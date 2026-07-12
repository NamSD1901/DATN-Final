import puppeteer from 'puppeteer';

(async () => {
  try {
    console.log("Launching browser...");
    const browser = await puppeteer.launch({ headless: true });
    const page = await browser.newPage();
    
    page.on('console', msg => {
      if (msg.type() === 'error') console.log('PAGE ERROR LOG:', msg.text());
    });
    page.on('pageerror', error => console.log('PAGE UNHANDLED ERROR:', error.message));

    console.log("Navigating to dashboard...");
    await page.goto('http://localhost:5173/dashboard');
    console.log("Waiting 3 seconds...");
    await new Promise(r => setTimeout(r, 3000));
    
    await browser.close();
  } catch(e) {
    console.error(e);
  }
})();
