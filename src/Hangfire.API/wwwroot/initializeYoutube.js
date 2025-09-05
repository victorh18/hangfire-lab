// Load the YouTube iFrame code
var apiScript = document.createElement("script");
apiScript.src = "https://www.youtube.com/iframe_api";

// Telling where in the document the iframe should be loaded
var iframeContainer = document.getElementsByTagName("main")[0];
iframeContainer.appendChild(apiScript);

const IFRAME_WIDTH = 640;

// Once the API code downloads, this gets executed:
var player;
function onYouTubeIframeAPIReady() {
    player = new YT.Player("player", {
        height: "390",
        width: IFRAME_WIDTH.toString(),
        videoId: "QG64mYP0d7o",
        playerVars: {
            playsinline: 1,
            origin: "http://127.0.0.1:5500",
        },
        events: {
            onReady: onPlayerReady,
            //'onStateChange': onPlayerStateChange
        },
    });
}

function onPlayerReady(event) {
    event.target.playVideo();
    event.target.mute();
}