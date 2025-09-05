//alert("hi mom")
const _URL = "https://localhost:7048/api/Extractor";

function getFormattedTimeStamp(_seconds) {
    const SECONDS_IN_MINUTES = 60;

    const minutes = Math.floor(_seconds / SECONDS_IN_MINUTES);
    const seconds = _seconds % SECONDS_IN_MINUTES;

    return `00:${minutes.toString().padStart(2, "0")}:${seconds.toString().padStart(2, "0")}`;
}

async function fetchVideo(url, startTime, endTime) {
    const formattedStartTime = getFormattedTimeStamp(startTime);
    const formattedEndTime = getFormattedTimeStamp(endTime);

    let request = {
        "id": "asdf",
        "videoUrl": `${url}`,
        "timeStamps": {
            "startTime": `${formattedStartTime}`,
            "endTime": `${formattedEndTime}`
        }
    }

    const headers = {
        "Content-Type": "application/json"
    }

    let config = {
        body: JSON.stringify(request),
        method: 'POST',
        headers
    };

    const response = await fetch(_URL, config);
    const responseData = await response.text();

    return responseData;
}

async function process() {
    const url = txtVideoUrl.value;
    const startTime = txtStartTime.value;
    const endTime = txtEndTime.value;

    const resultText = await fetchVideo(url, startTime, endTime);
    const resultNode = document.createElement('a');
    resultNode.href = `https://localhost:7048/api/File/${resultText}`;
    resultNode.innerText = "Download your file here!";
    //resultNode.download = resultText;
    conResult.appendChild(resultNode);
    conResult.appendChild(document.createElement("br"));
}

btnProcess.addEventListener('click', process);