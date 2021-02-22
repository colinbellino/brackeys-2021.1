const { parse } = require("node-html-parser");
const fetch = require("node-fetch");

exports.handler = async function(event, context)
{
    const authors = await extractAuthors();

    return {
        statusCode: 200,
        body: JSON.stringify({ authors }),
        headers: {
            "Access-Control-Allow-Origin": "*",
            "Access-Control-Allow-Headers": "Content-Type",
            "Access-Control-Allow-Methods": "GET",
        },
    };
}

function fetchComments()
{
    return fetch(process.env.GAME_URL)
        .then((response) => response.text())
        .catch((err) => { console.error('Error fetching the game page.', err); });
}

async function extractAuthors ()
{
    const html = await fetchComments();
    const root = parse(html);

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

    console.log(`Game: ${root.querySelector("h1.game_title").textContent}`);

    root.querySelector(".community_post_list_widget").childNodes.forEach(element =>
    {
        const author = element.querySelector(".post_author");
        const body = element.querySelector(".post_body");

        if (body == null) {
            return;
        }

        if (authors.includes(author.textContent) == false)
        {
            authors.push(author.textContent);
        }
    });

    // if (authors.length < fixtures.length) {
    //     authors = authors.concat(fixtures);
    // }

    return authors;
}
