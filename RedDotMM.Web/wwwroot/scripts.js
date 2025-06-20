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
    try {

    
    const tableBody = document.querySelector("#results-table tbody")
    tableBody.innerHTML = ""; // Tabelle leeren

    // SchuetzenName in das h3-Element einfügen
        const schuetzenNameElement = document.querySelector("h3.SchuetzeName");
    if (schuetzenNameElement && results.SchuetzeName) {
        schuetzenNameElement.textContent = results.SchuetzeName;
    }

        // Gesamtwertung in das h3-Element einfügen
        const gesamtWertungElement = document.querySelector("h3.GesamtWertung");
        if (gesamtWertungElement && results.GesamtWertung) {
            gesamtWertungElement.textContent = "Gesamt: " + results.GesamtWertung + " Ring";
        }

    // Schuesse-Liste in die Tabelle eintragen
        if (Array.isArray(results.Schuesse) && results.Schuesse.length > 0) {
            results.Schuesse.forEach(schuss => {
                const row = document.createElement("tr");
                row.innerHTML = `
                <td>${schuss.SchussNr ?? ""}</td>
                <td>${schuss.IsProbe ?? ""}</td>
                <td>${schuss.Ringzahl ?? ""}</td>             
            `;
                tableBody.appendChild(row);
            });
        } else
        {
            tableBody.innerHTML = "<tr><td colspan='3'>Keine Schüsse gefunden</td></tr>";
        }
    } catch (error) {
        console.error("Fehler beim Laden der Ergebnisse:", error);
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
