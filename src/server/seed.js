import { productsDb, planDowntimesDb, unplanDowntimesDb, initDatabase } from './db.js';

export async function seedDatabase() {
  initDatabase();

  // 1. Seed products jika kosong
  const productCount = await productsDb.countAsync({});
  if (productCount === 0) {
    const sampleProducts = [
      { productname: 'Mechanical Keyboard RGB', price: 850000, stok: 25, isavailable: 1, created_at: new Date().toISOString(), updated_at: new Date().toISOString() },
      { productname: 'Gaming Mouse Wireless', price: 420000, stok: 40, isavailable: 1, created_at: new Date().toISOString(), updated_at: new Date().toISOString() },
      { productname: 'Monitor LED 24 Inch 144Hz', price: 1950000, stok: 12, isavailable: 1, created_at: new Date().toISOString(), updated_at: new Date().toISOString() },
      { productname: 'Headset Surround 7.1', price: 560000, stok: 18, isavailable: 1, created_at: new Date().toISOString(), updated_at: new Date().toISOString() },
      { productname: 'Ergonomic Office Chair', price: 1450000, stok: 5, isavailable: 1, created_at: new Date().toISOString(), updated_at: new Date().toISOString() },
      { productname: 'USB-C Multiport Hub 7-in-1', price: 290000, stok: 0, isavailable: 0, created_at: new Date().toISOString(), updated_at: new Date().toISOString() },
      { productname: 'Webcam Full HD 1080p', price: 380000, stok: 15, isavailable: 1, created_at: new Date().toISOString(), updated_at: new Date().toISOString() },
      { productname: 'Deskmat Minimalist 90x40cm', price: 110000, stok: 35, isavailable: 1, created_at: new Date().toISOString(), updated_at: new Date().toISOString() },
      { productname: 'Standing Desk Dual Motor', price: 3200000, stok: 4, isavailable: 1, created_at: new Date().toISOString(), updated_at: new Date().toISOString() },
      { productname: 'Portable SSD 1TB NVMe', price: 1250000, stok: 0, isavailable: 0, created_at: new Date().toISOString(), updated_at: new Date().toISOString() },
      { productname: 'Microphone Condenser USB', price: 670000, stok: 8, isavailable: 1, created_at: new Date().toISOString(), updated_at: new Date().toISOString() },
      { productname: 'Laptop Stand Aluminum', price: 175000, stok: 22, isavailable: 1, created_at: new Date().toISOString(), updated_at: new Date().toISOString() },
    ];

    await productsDb.insertAsync(sampleProducts);
    console.log('[Seed] Inserted sample products into NeDB');
  }

  // 2. Seed plan downtimes jika kosong
  const planCount = await planDowntimesDb.countAsync({});
  if (planCount === 0) {
    const samplePlans = [
      { department: 'Maintenance', downtime_code: 'PM-001', description: 'Preventive maintenance mesin press bulanan', created_at: new Date().toISOString(), updated_at: new Date().toISOString() },
      { department: 'Production', downtime_code: 'CO-002', description: 'Changeover mould lini produksi A ke lini B', created_at: new Date().toISOString(), updated_at: new Date().toISOString() },
      { department: 'IT & Infrastructure', downtime_code: 'IT-101', description: 'Upgrade firmware router core dan server database', created_at: new Date().toISOString(), updated_at: new Date().toISOString() },
      { department: 'Facility', downtime_code: 'FC-005', description: 'Pembersihan cooling tower dan pengecekan chiller', created_at: new Date().toISOString(), updated_at: new Date().toISOString() },
      { department: 'Electrical', downtime_code: 'EL-003', description: 'Kalibrasi sensor suhu oven dan panel distribusi', created_at: new Date().toISOString(), updated_at: new Date().toISOString() },
      { department: 'Quality Assurance', downtime_code: 'QA-010', description: 'Kalibrasi tahunan instrumen ukur CMM', created_at: new Date().toISOString(), updated_at: new Date().toISOString() },
    ];

    await planDowntimesDb.insertAsync(samplePlans);
    console.log('[Seed] Inserted sample plan downtimes into NeDB');
  }

  // 3. Seed unplan downtimes jika kosong
  const unplanCount = await unplanDowntimesDb.countAsync({});
  if (unplanCount === 0) {
    const sampleUnplans = [
      { department: 'Production', downtime_code: 'BRK-101', description: 'Motor conveyor utama macet akibat bearing aus', created_at: new Date().toISOString(), updated_at: new Date().toISOString() },
      { department: 'Electrical', downtime_code: 'PWR-202', description: 'Trip breaker inverter akibat lonjakan arus sesaat', created_at: new Date().toISOString(), updated_at: new Date().toISOString() },
      { department: 'IT & Infrastructure', downtime_code: 'NET-305', description: 'Putusnya kabel fiber optic jalur gedung logistik', created_at: new Date().toISOString(), updated_at: new Date().toISOString() },
      { department: 'Maintenance', downtime_code: 'HYD-404', description: 'Kebocoran selang hidrolik pada unit clamp injeksi', created_at: new Date().toISOString(), updated_at: new Date().toISOString() },
      { department: 'Logistics', downtime_code: 'FLT-050', description: 'Forklift baterai drop di tengah proses unloading', created_at: new Date().toISOString(), updated_at: new Date().toISOString() },
      { department: 'Facility', downtime_code: 'AC-901', description: 'Kompresor AC ruang server mengalami overheating mendadak', created_at: new Date().toISOString(), updated_at: new Date().toISOString() },
    ];

    await unplanDowntimesDb.insertAsync(sampleUnplans);
    console.log('[Seed] Inserted sample unplan downtimes into NeDB');
  }
}

// Run seed directly if executed via cli
if (process.argv[1] && process.argv[1].endsWith('seed.js')) {
  seedDatabase().then(() => {
    console.log('[Seed] Database seeding completed successfully.');
  });
}
