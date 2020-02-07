async function fnImgMeme() {
    try {
        //console.meme("Foo", "Bar", "Good Guy Greg", 200, 150);
        console.meme("Ah", "Grasshopper", "https://i.imgur.com/6vhYZOq.jpg", 250, 250);
        console.log(
            "%chttps://bit.ly/BlazorRad",
            "color:red;font-family:system-ui;font-size:4rem;-webkit-text-stroke: 1px black;font-weight:bold"
        );
    } catch (e) {
        //Do nothing - doesn't matter if it failed
        console.log(e);//
    }
}
