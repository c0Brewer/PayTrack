import * as fs from 'fs';
import * as path from 'path';
import { fileURLToPath } from 'url';

import * as dotenv from 'dotenv';

// Fix for __dirname in ESM
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Load .env from repo root
const envPath = path.resolve(__dirname, '../../.env');
dotenv.config({ path: envPath });

// Decide output file for Angular environment
const targetPath = path.resolve(__dirname, './src/environments/environment.ts');

const environmentFileContent = `export const environment = {
  production: false,
  googleClientId: '${process.env.GOOGLE_CLIENT_ID}',
};
`;

fs.writeFileSync(targetPath, environmentFileContent, { encoding: 'utf8' });
