const puppeteer = require("puppeteer");

exports.handler = async function(event, context)
{
    const authors = await extractAuthors();

    return {
        statusCode: 200,
        body: JSON.stringify({ authors }),
    };
}

async function extractAuthors ()
{
    const fixtures = [
        "Micah",
        "Vernon",
        "Rena",
        "Riku",
        "Andre",
        "Thea",
        "Mariel",
        "Jesse",
        "Marceline",
        "Gaius",
        "Alma",
        "Ursula",
        "Celeste",
        "Madeline",
        "Thea",
    ];
    let authors = [];

    const browser = await puppeteer.launch({ executablePath: "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe" });
    const page = await browser.newPage();

    await page.goto(process.env.GAME_URL, { waitUntil: 'networkidle2' });
    const itemsLength = await page.evaluate((sel) => {
        return document.getElementsByClassName(sel).length;
      }, "post_author");

    for (let index = 0; index < itemsLength; index++) {
        const author = await page.$eval(`.community_post_list_widget :nth-child(${index+1}) .post_author`, el => el.innerText);
        if (authors.includes(author) == false) {
            authors.push(author);
        }
    }
  
    await browser.close();

    if (authors.length < fixtures.length) {
        authors = authors.concat(fixtures);
    }

    return authors;
}
