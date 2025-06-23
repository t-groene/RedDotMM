const apiUrl = "/api/ergebniss";
const apiBildUrl = "/api/bild";
const apiUpdateUrl = "/api/updateAvailable";
const apiFehlschussURL = "/api/Command/Fehlschuss";

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
            const response = await fetch(apiBildUrl);
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
        var summe = 0;

    // Schuesse-Liste in die Tabelle eintragen
        if (Array.isArray(results.Schuesse) && results.Schuesse.length > 0) {
            results.Schuesse.forEach(schuss => {

                var probe = schuss.IsProbe ? "Probe" : "Wertung";
                summe += schuss.Ringzahl ?? 0;
                const row = document.createElement("tr");
                row.innerHTML = `
                <td>${schuss.SchussNummer ?? ""}</td>
                <td>${probe ?? ""}</td>
                <td>${schuss.Ringzahl ?? ""}</td>             
            `;
                tableBody.appendChild(row);
            });


            const resultRow = document.createElement("tr");

            resultRow.innerHTML = `
                <td colspan="2">Summe:</td>
                <td>${summe}</td>
            `;
            tableBody.appendChild(resultRow);
        } else
        {
            tableBody.innerHTML = "<tr><td colspan='3'>Keine Schüsse gefunden</td></tr>";
        }
    } catch (error) {
        console.error("Fehler beim Laden der Ergebnisse:", error);
    }
}


// Funktion, um Daten Zyklisch zu aktualisieren
async function UpdateData() {
    //Prüfen, ob es Änderungen gibt;
    try {

        //Aktualisierungs-Indikator sichtbarkeit auf visible setzen
        const updateIndicator = document.querySelector("#update-indicator");
        if (updateIndicator) {
            updateIndicator.style.visibility = "visible";
        }
        

        const response = await fetch(apiUpdateUrl);
        if (!response.ok) {
            throw new Error("Fehler beim prüfen des Update-Status");
        }
        const data = await response.json();
        if (data.UpdateAvailable == true) {
            console.log("Update verfügbar, lade Ergebnisse neu...");
            loadResults();
        }

        //Aktualisierungs-Indikator sichtbarkeit auf hidden setzen
        if (updateIndicator) {
            updateIndicator.style.visibility = "hidden";
        }

    } catch (error) {
        console.error("Fehler beim prüfen des Update-Status", error);
    }


}


async function Fehlschuss()
{
    try {

        //Aktualisierungs-Indikator sichtbarkeit auf visible setzen
       
        const response = await fetch(apiFehlschussURL);
        if (!response.ok) {
            throw new Error("Fehler beim senden eines Fehlschusses");
        }
        
    } catch (error) {
        console.error("Fehler beim senden eines Fehlschusses:", error);
    }
}




// Initialisierung
document.addEventListener("DOMContentLoaded", () => {
    loadResults();
    setInterval(UpdateData, 1000);
});
