const apiUrl = "/api/ergebniss";
const webhookUrl = "/webhook"; // Webhook-Endpunkt

// Funktion, um Ergebnisse von der API zu laden
async function loadResults() {
    try {
        const response = await fetch(apiUrl);
        if (!response.ok) {
            throw new Error("Fehler beim Abrufen der Ergebnisse");
        }
        const data = await response.json();
        updateTable(data);
        updateImage();
    } catch (error) {
        console.error("Fehler beim Laden des Bildes:", error);
    }
}

    // Funktion, um das Bild von der API zu laden und das <img class="scheibe"/> zu aktualisieren
    async function updateImage() {
        try {
            const response = await fetch("/api/bild");
            if (!response.ok) {
                throw new Error("Fehler beim Abrufen des Bildes");
            }
            const arrayBuffer = await response.arrayBuffer();
            const blob = new Blob([arrayBuffer], { type: "image/png" });
            const imageUrl = URL.createObjectURL(blob);
            const imgElement = document.querySelector("img.scheibe");
            if (imgElement) {
                imgElement.src = imageUrl;
            }
        } catch (error) {
            console.error("Fehler beim Laden des Bildes:", error);
        }
    }


// Funktion, um die Tabelle mit Ergebnissen zu aktualisieren
function updateTable(results) {
    const tableBody = document.querySelector("#results-table tbody");
    tableBody.innerHTML = ""; // Tabelle leeren

    if (results) {
        const row = document.createElement("tr");
        row.innerHTML = `
            <td>${results.ergebnisId}</td>
            <td>${results.schuetze?.anzeigeName || "Unbekannt"}</td>
            <td>${new Date(results.zeitstempel).toLocaleString()}</td>
            <td>${results.anzahlWertungsschuesse}</td>
            <td>${results.anzahlProbeschuesse}</td>
        `;
        tableBody.appendChild(row);
    }
}

// Funktion, um den Webhook-Indikator anzuzeigen
function showWebhookIndicator() {
    const indicator = document.getElementById("webhook-indicator");
    indicator.classList.add("visible");
    setTimeout(() => {
        indicator.classList.remove("visible");
    }, 1000); // 1 Sekunde anzeigen
}

// WebSocket-Verbindung für den Webhook
function setupWebhookListener() {
    const eventSource = new EventSource(webhookUrl);
    eventSource.onmessage = (event) => {
        console.log("Webhook ausgelöst:", event.data);
        showWebhookIndicator();
        loadResults(); // Ergebnisse neu laden
    };

    eventSource.onerror = (error) => {
        console.error("Fehler beim Webhook:", error);
    };
}

// Initialisierung
document.addEventListener("DOMContentLoaded", () => {
    loadResults();
    setupWebhookListener();
});
