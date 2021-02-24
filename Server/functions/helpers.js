const { parse } = require("node-html-parser");
const fetch = require("node-fetch");

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

async function extractAuthors ()
{
    try {
        let authors = [];
    
        {
            const response = await fetch(process.env.GAME_URL);
            const text = await response.text();
            const root = parse(text);

            root.querySelector(".community_post_list_widget").childNodes.forEach(element =>
            {
                const author = element.querySelector(".post_author");
                console.log({author});
                
                if (author && authors.includes(author.textContent) == false)
                {
                    authors.push(author.textContent);
                }
            });
        }

        {
            const response = await fetch(process.env.SUBMISSION_URL);
            const text = await response.text();
            const root = parse(text);
        
            root.querySelector(".community_post_list_widget").childNodes.forEach(element =>
            {
                const author = element.querySelector(".post_author");
        
                if (author && authors.includes(author.textContent) == false)
                {
                    authors.push(author.textContent);
                }
            });
        }

        return authors;
    } catch(err) {
        console.error('Error fetching the game page.', err);
    }
}
