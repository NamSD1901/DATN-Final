const { Client } = require('pg');

async function checkDb() {
  const client = new Client({
    host: 'aws-1-ap-south-1.pooler.supabase.com',
    port: 5432,
    database: 'postgres',
    user: 'postgres.yzbacphijkzkjahlsbdz',
    password: 'cogangviphuonglamhanh',
    ssl: { rejectUnauthorized: false }
  });

  await client.connect();

  const res = await client.query(`SELECT work_date, start_time, is_available FROM doctor_schedules WHERE work_date >= '2026-07-13' AND work_date <= '2026-07-20' ORDER BY work_date ASC`).catch(e => console.log(e.message));
  if (res && res.rows) {
      console.log('Schedules between 13/07 and 20/07:');
      res.rows.forEach(r => console.log(r));
  }

  await client.end();
}

checkDb().catch(console.error);
