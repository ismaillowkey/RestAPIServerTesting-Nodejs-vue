export const swaggerSpec = {
  openapi: '3.0.3',
  info: {
    title: 'REST API Server (Node.js + SQLite)',
    version: '1.0.0',
    description: `API Dokumentasi untuk operasi CRUD kategori **Product**, **Plan Downtime**, dan **Unplan Downtime**.\n\nSemua endpoint READ mendukung query pagination opsional (\`page\`, \`limit\`, \`search\`).`
  },
  servers: [
    {
      url: '/',
      description: 'Current Server'
    }
  ],
  tags: [
    { name: 'Products', description: 'Operasi CRUD untuk Produk (id, productname, price, stok, isavailable)' },
    { name: 'Plan Downtimes', description: 'Operasi CRUD untuk Plan Downtime (id, department, downtime_code, description)' },
    { name: 'Unplan Downtimes', description: 'Operasi CRUD untuk Unplan Downtime (id, department, downtime_code, description)' }
  ],
  paths: {
    '/api/products': {
      get: {
        tags: ['Products'],
        summary: 'Mendapatkan daftar produk (Read all / Pagination opsional)',
        parameters: [
          { name: 'page', in: 'query', schema: { type: 'integer' }, description: 'Nomor halaman (opsional)' },
          { name: 'limit', in: 'query', schema: { type: 'integer', default: 10 }, description: 'Jumlah item per halaman (opsional)' },
          { name: 'search', in: 'query', schema: { type: 'string' }, description: 'Pencarian berdasarkan nama produk (opsional)' },
          { name: 'isavailable', in: 'query', schema: { type: 'integer', enum: [0, 1] }, description: 'Filter ketersediaan: 1 = Tersedia, 0 = Habis' }
        ],
        responses: {
          '200': {
            description: 'Daftar produk berhasil diambil',
            content: {
              'application/json': {
                schema: {
                  type: 'object',
                  properties: {
                    success: { type: 'boolean', example: true },
                    data: {
                      type: 'array',
                      items: { $ref: '#/components/schemas/Product' }
                    },
                    pagination: { $ref: '#/components/schemas/PaginationMeta' }
                  }
                }
              }
            }
          }
        }
      },
      post: {
        tags: ['Products'],
        summary: 'Menambahkan produk baru (Create)',
        requestBody: {
          required: true,
          content: {
            'application/json': {
              schema: { $ref: '#/components/schemas/ProductInput' }
            }
          }
        },
        responses: {
          '201': { description: 'Produk berhasil dibuat' },
          '400': { description: 'Validasi input gagal' }
        }
      }
    },
    '/api/products/{id}': {
      get: {
        tags: ['Products'],
        summary: 'Mendapatkan detail satu produk',
        parameters: [{ name: 'id', in: 'path', required: true, schema: { type: 'integer' } }],
        responses: {
          '200': { description: 'Data produk ditemukan' },
          '404': { description: 'Produk tidak ditemukan' }
        }
      },
      put: {
        tags: ['Products'],
        summary: 'Memperbarui data produk (Update)',
        parameters: [{ name: 'id', in: 'path', required: true, schema: { type: 'integer' } }],
        requestBody: {
          required: true,
          content: {
            'application/json': {
              schema: { $ref: '#/components/schemas/ProductInput' }
            }
          }
        },
        responses: {
          '200': { description: 'Produk berhasil diupdate' },
          '404': { description: 'Produk tidak ditemukan' }
        }
      },
      delete: {
        tags: ['Products'],
        summary: 'Menghapus produk (Delete)',
        parameters: [{ name: 'id', in: 'path', required: true, schema: { type: 'integer' } }],
        responses: {
          '200': { description: 'Produk berhasil dihapus' },
          '404': { description: 'Produk tidak ditemukan' }
        }
      }
    },
    '/api/plan-downtimes': {
      get: {
        tags: ['Plan Downtimes'],
        summary: 'Mendapatkan daftar plan downtime (Read all / Pagination opsional)',
        parameters: [
          { name: 'page', in: 'query', schema: { type: 'integer' }, description: 'Nomor halaman (opsional)' },
          { name: 'limit', in: 'query', schema: { type: 'integer', default: 10 }, description: 'Jumlah item per halaman (opsional)' },
          { name: 'search', in: 'query', schema: { type: 'string' }, description: 'Pencarian kode / deskripsi / departemen' },
          { name: 'department', in: 'query', schema: { type: 'string' }, description: 'Filter departemen' }
        ],
        responses: {
          '200': { description: 'Daftar plan downtime berhasil diambil' }
        }
      },
      post: {
        tags: ['Plan Downtimes'],
        summary: 'Menambahkan plan downtime baru (Create)',
        requestBody: {
          required: true,
          content: {
            'application/json': {
              schema: { $ref: '#/components/schemas/DowntimeInput' }
            }
          }
        },
        responses: {
          '201': { description: 'Plan downtime berhasil dibuat' },
          '400': { description: 'Validasi input gagal' }
        }
      }
    },
    '/api/plan-downtimes/{id}': {
      get: {
        tags: ['Plan Downtimes'],
        summary: 'Mendapatkan detail satu plan downtime',
        parameters: [{ name: 'id', in: 'path', required: true, schema: { type: 'integer' } }],
        responses: {
          '200': { description: 'Data ditemukan' },
          '404': { description: 'Tidak ditemukan' }
        }
      },
      put: {
        tags: ['Plan Downtimes'],
        summary: 'Memperbarui data plan downtime (Update)',
        parameters: [{ name: 'id', in: 'path', required: true, schema: { type: 'integer' } }],
        requestBody: {
          required: true,
          content: {
            'application/json': {
              schema: { $ref: '#/components/schemas/DowntimeInput' }
            }
          }
        },
        responses: {
          '200': { description: 'Berhasil diupdate' },
          '404': { description: 'Tidak ditemukan' }
        }
      },
      delete: {
        tags: ['Plan Downtimes'],
        summary: 'Menghapus data plan downtime (Delete)',
        parameters: [{ name: 'id', in: 'path', required: true, schema: { type: 'integer' } }],
        responses: {
          '200': { description: 'Berhasil dihapus' },
          '404': { description: 'Tidak ditemukan' }
        }
      }
    },
    '/api/unplan-downtimes': {
      get: {
        tags: ['Unplan Downtimes'],
        summary: 'Mendapatkan daftar unplan downtime (Read all / Pagination opsional)',
        parameters: [
          { name: 'page', in: 'query', schema: { type: 'integer' }, description: 'Nomor halaman (opsional)' },
          { name: 'limit', in: 'query', schema: { type: 'integer', default: 10 }, description: 'Jumlah item per halaman (opsional)' },
          { name: 'search', in: 'query', schema: { type: 'string' }, description: 'Pencarian kode / deskripsi / departemen' },
          { name: 'department', in: 'query', schema: { type: 'string' }, description: 'Filter departemen' }
        ],
        responses: {
          '200': { description: 'Daftar unplan downtime berhasil diambil' }
        }
      },
      post: {
        tags: ['Unplan Downtimes'],
        summary: 'Menambahkan unplan downtime baru (Create)',
        requestBody: {
          required: true,
          content: {
            'application/json': {
              schema: { $ref: '#/components/schemas/DowntimeInput' }
            }
          }
        },
        responses: {
          '201': { description: 'Unplan downtime berhasil dibuat' },
          '400': { description: 'Validasi input gagal' }
        }
      }
    },
    '/api/unplan-downtimes/{id}': {
      get: {
        tags: ['Unplan Downtimes'],
        summary: 'Mendapatkan detail satu unplan downtime',
        parameters: [{ name: 'id', in: 'path', required: true, schema: { type: 'integer' } }],
        responses: {
          '200': { description: 'Data ditemukan' },
          '404': { description: 'Tidak ditemukan' }
        }
      },
      put: {
        tags: ['Unplan Downtimes'],
        summary: 'Memperbarui data unplan downtime (Update)',
        parameters: [{ name: 'id', in: 'path', required: true, schema: { type: 'integer' } }],
        requestBody: {
          required: true,
          content: {
            'application/json': {
              schema: { $ref: '#/components/schemas/DowntimeInput' }
            }
          }
        },
        responses: {
          '200': { description: 'Berhasil diupdate' },
          '404': { description: 'Tidak ditemukan' }
        }
      },
      delete: {
        tags: ['Unplan Downtimes'],
        summary: 'Menghapus data unplan downtime (Delete)',
        parameters: [{ name: 'id', in: 'path', required: true, schema: { type: 'integer' } }],
        responses: {
          '200': { description: 'Berhasil dihapus' },
          '404': { description: 'Tidak ditemukan' }
        }
      }
    }
  },
  components: {
    schemas: {
      Product: {
        type: 'object',
        properties: {
          id: { type: 'integer', example: 1 },
          productname: { type: 'string', example: 'Mechanical Keyboard RGB' },
          price: { type: 'number', example: 850000 },
          stok: { type: 'integer', example: 25 },
          isavailable: { type: 'integer', enum: [0, 1], example: 1 },
          created_at: { type: 'string', example: '2026-09-15 14:00:00' },
          updated_at: { type: 'string', example: '2026-09-15 14:00:00' }
        }
      },
      ProductInput: {
        type: 'object',
        required: ['productname', 'price'],
        properties: {
          productname: { type: 'string', example: 'Mechanical Keyboard RGB' },
          price: { type: 'number', example: 850000 },
          stok: { type: 'integer', default: 0, example: 25 },
          isavailable: { type: 'integer', enum: [0, 1], default: 1, example: 1 }
        }
      },
      Downtime: {
        type: 'object',
        properties: {
          id: { type: 'integer', example: 1 },
          department: { type: 'string', example: 'Maintenance' },
          downtime_code: { type: 'string', example: 'PM-001' },
          description: { type: 'string', example: 'Preventive maintenance mesin press bulanan' },
          created_at: { type: 'string', example: '2026-09-15 14:00:00' },
          updated_at: { type: 'string', example: '2026-09-15 14:00:00' }
        }
      },
      DowntimeInput: {
        type: 'object',
        required: ['department', 'downtime_code'],
        properties: {
          department: { type: 'string', example: 'Maintenance' },
          downtime_code: { type: 'string', example: 'PM-001' },
          description: { type: 'string', example: 'Preventive maintenance mesin press bulanan' }
        }
      },
      PaginationMeta: {
        type: 'object',
        properties: {
          page: { type: 'integer', example: 1 },
          limit: { type: 'integer', example: 10 },
          total: { type: 'integer', example: 12 },
          totalPages: { type: 'integer', example: 2 },
          hasNext: { type: 'boolean', example: true },
          hasPrev: { type: 'boolean', example: false }
        }
      }
    }
  }
};
