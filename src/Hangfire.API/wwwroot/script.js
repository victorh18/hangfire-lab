const _URL = "https://localhost:7048/api/Extractor";

function getFormattedTimeStamp(_seconds) {
    const SECONDS_IN_MINUTES = 60;

    const minutes = Math.floor(_seconds / SECONDS_IN_MINUTES);
    const seconds = _seconds % SECONDS_IN_MINUTES;

    return `00:${minutes.toString().padStart(2, "0")}:${seconds.toString().padStart(2, "0")}`;
}

async function fetchVideo(url, startTime, endTime, extractionType = 0) {
    const formattedStartTime = getFormattedTimeStamp(startTime);
    const formattedEndTime = getFormattedTimeStamp(endTime);

    let request = {
        "id": "asf",
        "videoUrl": `${url}`,
        "timeStamps": {
            "startTime": `${formattedStartTime}`,
            "endTime": `${formattedEndTime}`
        },
        extractionType
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


function setWs(id) {
    const _url = `wss://localhost:7048/api/report/frontend/${id}`;
    let ws = new WebSocket(_url);
    let buttonAppended = false;

    ws.onopen = () => {
        console.log(`Connected to progress report for ${id}`);
    };

    ws.onmessage = (event) => {
        console.log(`Received report item for ${id}:`, event);
        let progress = event.data.split(":")[1].trim();
        prgProcess.value = progress

        if (event.data == "Processing: 100" && buttonAppended == false) {
            const downloadButton = getDownloadButton(id)
            conResult.appendChild(downloadButton)
            buttonAppended = true
            progressContainer.innerText = "";

        }
    };

    ws.onerror = (err) => {
        console.error(`WebSocket error for ${id}`, err);
    };
}

function getDownloadButton(id) {
    const downloadButton = document.createElement('a');
    const downloadSpan = document.createElement('span');
    downloadSpan.innerText = "Download!"
    downloadButton.href = `https://localhost:7048/api/File/${id}`
    downloadButton.className = 'flex justify-center items-center w-full bg-[var(--accent-color)] h-12 text-white font-bold transition-all rounded-lg hover:bg-cyan-300 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-offset-[var(--bg-color)] focus:ring-[var(--accent-color)]'
    downloadButton.appendChild(downloadSpan);
    return downloadButton;
}

async function process() {
    const url = txtVideoUrl.value;
    const startTime = txtStartTime.value;
    const endTime = txtEndTime.value;
    const extractionType = parseInt(drpExtractionType.value)

    conResult.innerText = "";
    progressContainer.innerText = "";

    setProgressBar()

    const resultText = await fetchVideo(url, startTime, endTime, extractionType);
    setWs(resultText)
    const resultNode = document.createElement('a');
    resultNode.href = `https://localhost:7048/api/File/${resultText}`;
    resultNode.innerText = "Download your file here!";
}

function setProgressBar() {
    let label = document.createElement('label');
    label.innerText = "Processing...";

    let progressBar = document.createElement('progress');
    progressBar.className = "w-full";
    progressBar.id = 'prgProcess';
    progressBar.max = 100;
    progressBar.value = 0;

    progressContainer.appendChild(label);
    progressContainer.appendChild(progressBar);
}

btnProcess.addEventListener('click', process);