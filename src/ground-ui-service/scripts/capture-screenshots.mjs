#!/usr/bin/env node
import { chromium } from "playwright";
import { mkdir } from "fs/promises";
import { join, dirname } from "path";
import { fileURLToPath } from "url";

const __dirname = dirname(fileURLToPath(import.meta.url));
const BASE_URL = process.env.BASE_URL || "http://localhost:5180";
const OUT_DIR = join(__dirname, "..", "..", "..", "docs", "ui");

const VIEWPORT = { width: 1280, height: 800 };
const MAP_VIEWPORT = { width: 1280, height: 1050 };

const MISSION_ID = process.env.MISSION_ID || "e8e0d1b0-1a2b-4c5d-9e8f-7a6b5c4d3e2f";
const SATELLITE_ID = process.env.SATELLITE_ID || "f47ac10b-58cc-4372-a567-0e02b2c3d479";

const MISSION_TAB_LABELS = ["Satellites", "Map", "Command queue", "Activity"];
const SATELLITE_TAB_LABELS = ["Activity", "Commands", "Simulator"];

async function scrollOverflows(page) {
  await page.evaluate(() => {
    document.querySelectorAll(".overflow-y-auto").forEach((el) => {
      el.scrollTop = el.scrollHeight;
    });
  });
  await page.waitForTimeout(300);
}

function scrollComponentOverflows(page, ratio) {
  return page.evaluate((r) => {
    document.querySelectorAll(".overflow-y-auto").forEach((el) => {
      const maxScroll = el.scrollHeight - el.clientHeight;
      el.scrollTop = Math.max(0, Math.floor(maxScroll * r));
    });
  }, ratio);
}

async function hideScrollbarsForScreenshot(page) {
  await page.addStyleTag({
    content: "html, body { overflow-x: hidden !important; } * { scrollbar-width: none !important; } *::-webkit-scrollbar { display: none !important; }",
  });
  await page.waitForTimeout(100);
}

async function main() {
  await mkdir(OUT_DIR, { recursive: true });
  console.log("Screenshots will be saved to:", OUT_DIR);
  console.log("Base URL:", BASE_URL);

  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    viewport: VIEWPORT,
    ignoreHTTPSErrors: true,
  });

  const page = await context.newPage();

  const singlePages = [
    { path: "/", name: "01-home" },
    { path: "/missions", name: "02-missions-list" },
    { path: "/missions/new", name: "03-mission-form" },
    { path: "/satellites", name: "08-satellites-list" },
    { path: "/satellites/new", name: "09-satellite-form" },
  ];

  for (const { path, name } of singlePages) {
    try {
      await page.goto(BASE_URL + path, { waitUntil: "load", timeout: 25000 });
      await page.waitForTimeout(1200);
      await hideScrollbarsForScreenshot(page);
      const file = join(OUT_DIR, `${name}.png`);
      await page.screenshot({ path: file, fullPage: true });
      console.log("  OK", path, "->", `${name}.png`);
    } catch (err) {
      console.warn("  SKIP", path, err.message);
    }
  }

  const missionTabIds = ["satellites", "map", "commands", "activity"];
  try {
    await page.goto(BASE_URL + `/missions/${MISSION_ID}`, { waitUntil: "load", timeout: 25000 });
    await page.waitForTimeout(3000);
    await page.getByRole("button", { name: "Satellites" }).waitFor({ state: "visible", timeout: 15000 });
    await hideScrollbarsForScreenshot(page);

    for (let i = 0; i < MISSION_TAB_LABELS.length; i++) {
      const label = MISSION_TAB_LABELS[i];
      const tabId = missionTabIds[i];
      await page.getByRole("button", { name: label }).click({ timeout: 10000 });
      await page.waitForTimeout(600);
      if (tabId === "map") {
        const zoomOut = page.locator(".leaflet-control-zoom-out");
        if (await zoomOut.count() > 0) {
          for (let j = 0; j < 5; j++) {
            await zoomOut.click();
            await page.waitForTimeout(500);
          }
        }
        await page.waitForTimeout(300);
        await page.setViewportSize(MAP_VIEWPORT);
        await page.waitForTimeout(200);
        const file = join(OUT_DIR, "05-mission-details-map-tab.png");
        await page.screenshot({ path: file, fullPage: false });
        await page.setViewportSize(VIEWPORT);
        console.log("  OK", tabId, "->", "05-mission-details-map-tab.png");
      } else {
        await scrollOverflows(page);
        const num = { satellites: "04", commands: "06", activity: "07" }[tabId];
        const file = join(OUT_DIR, `${num}-mission-details-${tabId}-tab.png`);
        await page.screenshot({ path: file, fullPage: true });
        console.log("  OK", tabId, "->", `${num}-mission-details-${tabId}-tab.png`);
      }
    }
  } catch (err) {
    console.warn("  SKIP mission details tabs", err.message);
  }

  const satelliteTabIds = ["activity", "commands", "simulator"];
  try {
    await page.goto(BASE_URL + `/satellites/${SATELLITE_ID}`, { waitUntil: "load", timeout: 25000 });
    await page.getByRole("button", { name: "Activity" }).waitFor({ state: "visible", timeout: 15000 });
    console.log("  Waiting 90s for healthcheck events…");
    await page.waitForTimeout(90 * 1000);
    await hideScrollbarsForScreenshot(page);

    for (let i = 0; i < SATELLITE_TAB_LABELS.length; i++) {
      const label = SATELLITE_TAB_LABELS[i];
      const tabId = satelliteTabIds[i];
      await page.getByRole("button", { name: label }).click({ timeout: 10000 });
      await page.waitForTimeout(600);

      if (tabId === "activity" || tabId === "commands") {
        const baseNum = tabId === "activity" ? "10" : "11";
        await page.evaluate(() => window.scrollTo(0, 0));
        await scrollComponentOverflows(page, 0);
        await page.waitForTimeout(300);
        const file1 = join(OUT_DIR, `${baseNum}.1-satellite-page-${tabId}-tab.png`);
        await page.screenshot({ path: file1, fullPage: tabId === "activity" });
        console.log("  OK", tabId, "->", `${baseNum}.1-satellite-page-${tabId}-tab.png`);

        await scrollComponentOverflows(page, 0.5);
        if (tabId === "commands") {
          const halfY = await page.evaluate(() => Math.max(0, Math.floor((document.documentElement.scrollHeight - window.innerHeight) / 2)));
          await page.evaluate((y) => window.scrollTo(0, y), halfY);
        }
        await page.waitForTimeout(300);
        const file2 = join(OUT_DIR, `${baseNum}.2-satellite-page-${tabId}-tab.png`);
        await page.screenshot({ path: file2, fullPage: tabId === "activity" });
        console.log("  OK", tabId, "->", `${baseNum}.2-satellite-page-${tabId}-tab.png`);

        if (tabId === "activity") {
          await scrollComponentOverflows(page, 1);
          await page.waitForTimeout(300);
          const file3 = join(OUT_DIR, "10.3-satellite-page-activity-tab.png");
          await page.screenshot({ path: file3, fullPage: true });
          console.log("  OK", tabId, "->", "10.3-satellite-page-activity-tab.png");
        }
      } else {
        await scrollOverflows(page);
        const file = join(OUT_DIR, `12-satellite-page-${tabId}-tab.png`);
        await page.screenshot({ path: file, fullPage: true });
        console.log("  OK", tabId, "->", `12-satellite-page-${tabId}-tab.png`);
      }
    }
  } catch (err) {
    console.warn("  SKIP satellite details tabs", err.message);
  }

  await browser.close();
  console.log("Done.");
}

main().catch((err) => {
  console.error(err);
  process.exit(1);
});
