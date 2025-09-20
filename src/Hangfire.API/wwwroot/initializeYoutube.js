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

    txtVideoUrl.addEventListener('paste', resetYoutubeFrame)
}

function onPlayerReady(event) {
    //event.target.playVideo();
    event.target.mute();
}

function resetYoutubeFrame(e) {
    const url = e.clipboardData.getData("text")
    console.log('pasted value: ', url);
    const videoId = url.split("v=")[1];
    player.loadVideoById(videoId);
}

function pauseVideo() {
    player.pauseVideo();
}

function playSection() {
    var startTime = Number(txtStartTime.value);
    var endTime = Number(txtEndTime.value);

    var difference = endTime - startTime

    player.seekTo(startTime, true);
    player.playVideo()

    setTimeout(pauseVideo, difference * 1000)
}

btnPlay.addEventListener('click', playSection)