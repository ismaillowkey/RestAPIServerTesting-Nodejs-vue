import express from 'express';
import cors from 'cors';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import swaggerUi from 'swagger-ui-express';
import { config } from '../config.js';
import { initDatabase } from './db.js';
import { seedDatabase } from './seed.js';
import { swaggerSpec } from './swagger.js';
import productsRouter from './routes/products.js';
import planDowntimesRouter from './routes/planDowntimes.js';
import unplanDowntimesRouter from './routes/unplanDowntimes.js';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Initialize SQLite database and seed initial data if needed
initDatabase();
seedDatabase();

const app = express();

// Standard Middlewares
app.use(cors());
app.use(express.json());

// API Routes
app.use('/api/products', productsRouter);
app.use('/api/plan-downtimes', planDowntimesRouter);
app.use('/api/unplan-downtimes', unplanDowntimesRouter);

// Swagger / OpenAPI documentation
app.get('/api/openapi.json', (req, res) => {
  res.json(swaggerSpec);
});
app.use('/api/docs', swaggerUi.serve, swaggerUi.setup(swaggerSpec, {
  customSiteTitle: 'REST API Documentation'
}));

// Quick health check
app.get('/api/health', (req, res) => {
  res.json({
    status: 'ok',
    uptime: process.uptime(),
    timestamp: new Date().toISOString()
  });
});

// Single-port Vue frontend integration:
// In development: Vite middleware handles hot-module reload on the same port!
// In production: Express serves compiled client/dist
import fs from 'node:fs';

const staticInDist = path.resolve(__dirname, 'public');
const isRunningFromDist = fs.existsSync(path.join(staticInDist, 'index.html'));
const isProduction = isRunningFromDist || config.env === 'production' || process.env.NODE_ENV === 'production';

async function startServer() {
  if (!isProduction) {
    const { createServer: createViteServer } = await import('vite');
    const vite = await createViteServer({
      server: { middlewareMode: true },
      appType: 'spa',
      root: path.resolve(__dirname, '../client')
    });
    app.use(vite.middlewares);
  } else {
    const possibleStaticDirs = [
      path.resolve(__dirname, 'public'),
      path.resolve(__dirname, '../dist/public'),
      path.resolve(process.cwd(), 'dist/public'),
      path.resolve(__dirname, '../client/dist'),
      path.resolve(__dirname, 'client/dist')
    ];
    const clientDist = possibleStaticDirs.find(dir => fs.existsSync(path.join(dir, 'index.html'))) || possibleStaticDirs[0];
    console.log(`[Static] Serving frontend from: ${clientDist}`);
    app.use(express.static(clientDist));
    app.use((req, res) => {
      res.sendFile(path.join(clientDist, 'index.html'));
    });
  }

  const server = app.listen(config.port, () => {
    console.log('====================================================');
    console.log(`🚀 Web Server berjalan di: http://localhost:${config.port}`);
    console.log(`📦 REST API Product:        http://localhost:${config.port}/api/products`);
    console.log(`📅 REST API Plan Downtime:  http://localhost:${config.port}/api/plan-downtimes`);
    console.log(`⚠️  REST API Unplan Downtime:http://localhost:${config.port}/api/unplan-downtimes`);
    console.log(`📖 Swagger API Docs:        http://localhost:${config.port}/api/docs`);
    console.log(`⚙️  Konfigurasi Port di:     config.js (PORT = ${config.port})`);
    console.log('====================================================');
  });

  return server;
}

startServer();
export { app };
