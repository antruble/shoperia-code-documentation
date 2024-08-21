const openFiles = [];
let openedFileId = -1;

document.addEventListener('DOMContentLoaded', async function () {
    const savedFiles = JSON.parse(localStorage.getItem('openFiles') || '[]');
    if (savedFiles.length > 0) {
        document.getElementById('minimizedModalButton').classList.remove('hidden');
    }
    for (const savedFile of savedFiles) {
        if (!openFiles.some(file => file.id === savedFile.id)) {
            await createTab(savedFile);
        }
    }

    // Event listenerek hozz�ad�sa a file linkekhez
    document.querySelectorAll('.file-name').forEach(link => {
        link.addEventListener('click',async function (e) {
            e.preventDefault();
            const fileId = this.getAttribute('data-file-id');
            const fileName = this.getAttribute('data-file-name');

            //fetchFileContent(fileId, fileName);

            // �J SOROK
            await openFile({id: fileId, name: fileName});
        });
    });
});

function saveOpenFilesToLocalStorage() {
    localStorage.setItem('openFiles', JSON.stringify(openFiles));
}
async function openFile(file) {
    console.log(`Megnyitom a ${file.id} ID-j� ${file.name} nev� f�jlt`);
    await createTab(file);
    selectTab(file);
    await loadContent(file.id);
    // MODAL MEGNYIT�SA
    document.getElementById('modal').classList.remove('hidden');
}
async function createTab(file) {
    console.log(`MEGN�ZEM HOGY KELL E L�TREHOZNOM NEKI TABOT: ${!openFiles.some(f => f.id === file.id) }`);
    if (!openFiles.some(f => f.id === file.id)) {
        openFiles.push(file);
        saveOpenFilesToLocalStorage();
        console.log(`beleraktam az openFiles t�mbbe a ${file.id} ID-T`);
        const tabs = document.getElementById('tabs');
        const tab = document.createElement('div');
        tab.className = 'tab p-2 bg-gray-800 text-white rounded';
        tab.dataset.id = file.id;
        tab.innerText = file.name;
        tab.onclick = async () => await selectTab(file);
        tabs.appendChild(tab);
    }
}
async function selectTab(file) {
    console.log(`EDDIG A ${openedFileId} volt megnyitva, most a ${file.id}-t akarom, teh�t ez a kett� egyenl�? : ${Number(openedFileId) !== Number(file.id) }`); 
    if (Number(openedFileId) !== Number(file.id)) {
        const tabs = document.getElementById('tabs');
        const allTabs = Array.from(tabs.querySelectorAll('.tab'));

        openedFileId = file.id;
        console.log(`BE�LLYTOM KIV�LASZTOTTNAK AZ ID: ${file.id}`);
        // V�lasszuk ki a megfelel� tabot
        allTabs.forEach(tab => {
            if (Number(tab.dataset.id) === Number(file.id)) {
                tab.classList.add('active');
            } else {
                tab.classList.remove('active');
            }
        });
        await loadContent(file.id);
    }
}
async function loadContent(fileId) {    
    // fetch the data
    const data = await fetchFileContent(fileId);
    if (data !== null) {
        const modalContent = document.getElementById('modalContent');
        modalContent.innerHTML = `<div id="content-${fileId}" class="text-black">${data}</div>`;
    }
}
function closeModal() {
    openFiles.length = 0;
    localStorage.removeItem('openFiles');
    const modal = document.getElementById('modal');
    modal.classList.add('hidden');
    document.getElementById('tabs').innerHTML = ''; // Ki�r�ti a tabokat
    document.getElementById('modalContent').innerHTML = ''; // Ki�r�ti a modal tartalm�t
}

function minimizeModal() {
    const modal = document.getElementById('modal');
    const minimizedButton = document.getElementById('minimizedModalButton');
    modal.classList.add('hidden');
    minimizedButton.classList.remove('hidden');
}

function restoreModal() {
    const modal = document.getElementById('modal');
    const minimizedButton = document.getElementById('minimizedModalButton');
    modal.classList.remove('hidden');
    minimizedButton.classList.add('hidden');
}

async function fetchFileContent(fileId) {
    try {
        const response = await fetch(`/ClassTree/GetFileContent?fileId=${fileId}`);
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }

        return await response.text();
    } catch (error) {
        console.error('There was a problem with the fetch operation:', error);
        return null;
    }
}
