const { parse } = require("node-html-parser");
const fetch = require("node-fetch");

const GAME_URL = "https://this-is-a-test.itch.io/test-1";

function fetchComments()
{
    return fetch(GAME_URL)
        .then((response) => response.text())
        .catch((err) => { console.error('Error fetching the game page.', err); });
}

async function bla ()
{
    const html = await fetchComments();
    const root = parse(html);

    const authors = [];
    
    console.log(root.querySelector("h1.game_title").textContent);

    // console.log(root.querySelector(".community_post_list_widget").childNodes);

    root.querySelector(".community_post_list_widget").childNodes.forEach(element =>
    {
        const body = element.querySelector(".post_author");

        if (body == null) {
            return;
        }

        if (authors.includes(body.textContent) == false)
        {
            authors.push(body.textContent);
        }
    });

    console.log("authors:");
    console.table(authors);
}

bla();
