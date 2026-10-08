import { Router } from 'express';
import { productsDb, formatDoc } from '../db.js';

const router = Router();

// GET /api/products - Read with optional pagination & search
router.get('/', async (req, res) => {
  try {
    const { page, limit = 10, search = '', isavailable } = req.query;

    const query = {};

    if (search && search.trim()) {
      // Regex case-insensitive search
      query.productname = new RegExp(search.trim(), 'i');
    }

    if (isavailable !== undefined && isavailable !== '') {
      query.isavailable = Number(isavailable);
    }

    const totalCount = await productsDb.countAsync(query);

    // If page parameter is provided, perform pagination
    if (page !== undefined && page !== null && page !== '') {
      const pageNum = Math.max(1, parseInt(page, 10) || 1);
      const limitNum = Math.max(1, parseInt(limit, 10) || 10);
      const skip = (pageNum - 1) * limitNum;

      const rawData = await productsDb.findAsync(query)
        .sort({ created_at: -1, _id: -1 })
        .skip(skip)
        .limit(limitNum);

      const data = formatDoc(rawData);
      const totalPages = Math.ceil(totalCount / limitNum) || 1;

      return res.json({
        success: true,
        data,
        pagination: {
          page: pageNum,
          limit: limitNum,
          total: totalCount,
          totalPages,
          hasNext: pageNum < totalPages,
          hasPrev: pageNum > 1
        }
      });
    }

    // Without pagination query: return all records
    const rawData = await productsDb.findAsync(query).sort({ created_at: -1, _id: -1 });
    const data = formatDoc(rawData);

    return res.json({
      success: true,
      data,
      total: totalCount
    });
  } catch (err) {
    console.error('[Error GET /api/products]:', err);
    return res.status(500).json({ success: false, message: err.message });
  }
});

// GET /api/products/:id - Read single product
router.get('/:id', async (req, res) => {
  try {
    const { id } = req.params;
    const item = await productsDb.findOneAsync({ $or: [{ _id: id }, { id: id }] });

    if (!item) {
      return res.status(404).json({ success: false, message: 'Product tidak ditemukan' });
    }

    return res.json({ success: true, data: formatDoc(item) });
  } catch (err) {
    console.error('[Error GET /api/products/:id]:', err);
    return res.status(500).json({ success: false, message: err.message });
  }
});

// POST /api/products - Create product
router.post('/', async (req, res) => {
  try {
    const { productname, price, stok, isavailable } = req.body;

    if (!productname || productname.trim() === '') {
      return res.status(400).json({ success: false, message: 'Nama produk (productname) wajib diisi' });
    }

    const priceNum = Number(price);
    if (isNaN(priceNum) || priceNum < 0) {
      return res.status(400).json({ success: false, message: 'Harga (price) harus berupa angka valid >= 0' });
    }

    const stokNum = parseInt(stok, 10);
    const validStok = isNaN(stokNum) ? 0 : Math.max(0, stokNum);
    const validAvailable = (isavailable === true || isavailable === 1 || isavailable === '1' || isavailable === 'true') ? 1 : 0;

    const newDoc = {
      productname: productname.trim(),
      price: priceNum,
      stok: validStok,
      isavailable: validAvailable,
      created_at: new Date().toISOString(),
      updated_at: new Date().toISOString()
    };

    const inserted = await productsDb.insertAsync(newDoc);

    return res.status(201).json({
      success: true,
      message: 'Product berhasil ditambahkan',
      data: formatDoc(inserted)
    });
  } catch (err) {
    console.error('[Error POST /api/products]:', err);
    return res.status(500).json({ success: false, message: err.message });
  }
});

// PUT /api/products/:id - Update product
router.put('/:id', async (req, res) => {
  try {
    const { id } = req.params;
    const existing = await productsDb.findOneAsync({ $or: [{ _id: id }, { id: id }] });

    if (!existing) {
      return res.status(404).json({ success: false, message: 'Product tidak ditemukan' });
    }

    const { productname, price, stok, isavailable } = req.body;

    const updatedName = productname !== undefined ? productname.trim() : existing.productname;
    if (!updatedName) {
      return res.status(400).json({ success: false, message: 'Nama produk (productname) tidak boleh kosong' });
    }

    const updatedPrice = price !== undefined ? Number(price) : existing.price;
    if (isNaN(updatedPrice) || updatedPrice < 0) {
      return res.status(400).json({ success: false, message: 'Harga (price) harus angka valid >= 0' });
    }

    const updatedStok = stok !== undefined ? parseInt(stok, 10) : existing.stok;
    const finalStok = isNaN(updatedStok) ? 0 : Math.max(0, updatedStok);

    const updatedAvailable = isavailable !== undefined
      ? ((isavailable === true || isavailable === 1 || isavailable === '1' || isavailable === 'true') ? 1 : 0)
      : existing.isavailable;

    await productsDb.updateAsync(
      { _id: existing._id },
      {
        $set: {
          productname: updatedName,
          price: updatedPrice,
          stok: finalStok,
          isavailable: updatedAvailable,
          updated_at: new Date().toISOString()
        }
      }
    );

    const updatedItem = await productsDb.findOneAsync({ _id: existing._id });

    return res.json({
      success: true,
      message: 'Product berhasil diperbarui',
      data: formatDoc(updatedItem)
    });
  } catch (err) {
    console.error('[Error PUT /api/products/:id]:', err);
    return res.status(500).json({ success: false, message: err.message });
  }
});

// DELETE /api/products/:id - Delete product
router.delete('/:id', async (req, res) => {
  try {
    const { id } = req.params;
    const existing = await productsDb.findOneAsync({ $or: [{ _id: id }, { id: id }] });

    if (!existing) {
      return res.status(404).json({ success: false, message: 'Product tidak ditemukan' });
    }

    await productsDb.removeAsync({ _id: existing._id }, {});

    return res.json({
      success: true,
      message: 'Product berhasil dihapus',
      data: formatDoc(existing)
    });
  } catch (err) {
    console.error('[Error DELETE /api/products/:id]:', err);
    return res.status(500).json({ success: false, message: err.message });
  }
});

export default router;
