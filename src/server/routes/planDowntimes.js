import { Router } from 'express';
import { planDowntimesDb, formatDoc } from '../db.js';

const router = Router();

// GET /api/plan-downtimes - Read with optional pagination & search
router.get('/', async (req, res) => {
  try {
    const { page, limit = 10, search = '', department } = req.query;

    const query = {};

    if (search && search.trim()) {
      const reg = new RegExp(search.trim(), 'i');
      query.$or = [
        { downtime_code: reg },
        { description: reg },
        { department: reg }
      ];
    }

    if (department && department.trim() !== '') {
      query.department = department.trim();
    }

    const totalCount = await planDowntimesDb.countAsync(query);

    if (page !== undefined && page !== null && page !== '') {
      const pageNum = Math.max(1, parseInt(page, 10) || 1);
      const limitNum = Math.max(1, parseInt(limit, 10) || 10);
      const skip = (pageNum - 1) * limitNum;

      const rawData = await planDowntimesDb.findAsync(query)
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

    const rawData = await planDowntimesDb.findAsync(query).sort({ created_at: -1, _id: -1 });
    const data = formatDoc(rawData);

    return res.json({
      success: true,
      data,
      total: totalCount
    });
  } catch (err) {
    console.error('[Error GET /api/plan-downtimes]:', err);
    return res.status(500).json({ success: false, message: err.message });
  }
});

// GET /api/plan-downtimes/:id - Read single plan downtime
router.get('/:id', async (req, res) => {
  try {
    const { id } = req.params;
    const item = await planDowntimesDb.findOneAsync({ $or: [{ _id: id }, { id: id }] });

    if (!item) {
      return res.status(404).json({ success: false, message: 'Plan Downtime tidak ditemukan' });
    }

    return res.json({ success: true, data: formatDoc(item) });
  } catch (err) {
    console.error('[Error GET /api/plan-downtimes/:id]:', err);
    return res.status(500).json({ success: false, message: err.message });
  }
});

// POST /api/plan-downtimes - Create plan downtime
router.post('/', async (req, res) => {
  try {
    const { department, downtime_code, description } = req.body;

    if (!department || department.trim() === '') {
      return res.status(400).json({ success: false, message: 'Department wajib diisi' });
    }

    if (!downtime_code || downtime_code.trim() === '') {
      return res.status(400).json({ success: false, message: 'Kode downtime (downtime_code) wajib diisi' });
    }

    const newDoc = {
      department: department.trim(),
      downtime_code: downtime_code.trim().toUpperCase(),
      description: description ? description.trim() : '',
      created_at: new Date().toISOString(),
      updated_at: new Date().toISOString()
    };

    const inserted = await planDowntimesDb.insertAsync(newDoc);

    return res.status(201).json({
      success: true,
      message: 'Plan Downtime berhasil ditambahkan',
      data: formatDoc(inserted)
    });
  } catch (err) {
    console.error('[Error POST /api/plan-downtimes]:', err);
    return res.status(500).json({ success: false, message: err.message });
  }
});

// PUT /api/plan-downtimes/:id - Update plan downtime
router.put('/:id', async (req, res) => {
  try {
    const { id } = req.params;
    const existing = await planDowntimesDb.findOneAsync({ $or: [{ _id: id }, { id: id }] });

    if (!existing) {
      return res.status(404).json({ success: false, message: 'Plan Downtime tidak ditemukan' });
    }

    const { department, downtime_code, description } = req.body;

    const updatedDept = department !== undefined ? department.trim() : existing.department;
    if (!updatedDept) {
      return res.status(400).json({ success: false, message: 'Department tidak boleh kosong' });
    }

    const updatedCode = downtime_code !== undefined ? downtime_code.trim().toUpperCase() : existing.downtime_code;
    if (!updatedCode) {
      return res.status(400).json({ success: false, message: 'Kode downtime tidak boleh kosong' });
    }

    const updatedDesc = description !== undefined ? description.trim() : existing.description;

    await planDowntimesDb.updateAsync(
      { _id: existing._id },
      {
        $set: {
          department: updatedDept,
          downtime_code: updatedCode,
          description: updatedDesc,
          updated_at: new Date().toISOString()
        }
      }
    );

    const updatedItem = await planDowntimesDb.findOneAsync({ _id: existing._id });

    return res.json({
      success: true,
      message: 'Plan Downtime berhasil diperbarui',
      data: formatDoc(updatedItem)
    });
  } catch (err) {
    console.error('[Error PUT /api/plan-downtimes/:id]:', err);
    return res.status(500).json({ success: false, message: err.message });
  }
});

// DELETE /api/plan-downtimes/:id - Delete plan downtime
router.delete('/:id', async (req, res) => {
  try {
    const { id } = req.params;
    const existing = await planDowntimesDb.findOneAsync({ $or: [{ _id: id }, { id: id }] });

    if (!existing) {
      return res.status(404).json({ success: false, message: 'Plan Downtime tidak ditemukan' });
    }

    await planDowntimesDb.removeAsync({ _id: existing._id }, {});

    return res.json({
      success: true,
      message: 'Plan Downtime berhasil dihapus',
      data: formatDoc(existing)
    });
  } catch (err) {
    console.error('[Error DELETE /api/plan-downtimes/:id]:', err);
    return res.status(500).json({ success: false, message: err.message });
  }
});

export default router;
