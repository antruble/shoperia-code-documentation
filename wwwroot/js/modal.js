let openFiles = [];
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

    // Event listenerek hozzáadása a file linkekhez
    document.querySelectorAll('.file-name').forEach(link => {
        link.addEventListener('click',async function (e) {
            e.preventDefault();
            const fileId = this.getAttribute('data-file-id');
            const fileName = this.getAttribute('data-file-name');
            // ÚJ SOROK
            await openFile({id: fileId, name: fileName});
        });
    });
});
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

function saveOpenFilesToLocalStorage() {
    localStorage.setItem('openFiles', JSON.stringify(openFiles));
}
async function openFile(file) {
    await createTab(file);
    selectTab(file);
    await loadContent(file.id);
    // MODAL MEGNYITÁSA
    document.getElementById('modal').classList.remove('hidden');
}
async function createTab(file) {
    if (!openFiles.some(f => f.id === file.id)) {
        openFiles.push(file);
        saveOpenFilesToLocalStorage();

        const tabs = document.getElementById('tabs');
        const tab = document.createElement('div');
        tab.className = 'tab p-2 bg-gray-800 text-white rounded flex items-center';
        tab.dataset.id = file.id;

        const tabName = document.createElement('span');
        tabName.innerText = file.name;
        tabName.onclick = async () => await selectTab(file);
        tab.appendChild(tabName);

        const closeButton = document.createElement('button');
        closeButton.innerText = 'X';
        closeButton.className = 'ml-2 text-red-500 hover:text-red-700';
        closeButton.onclick = (e) => {
            e.stopPropagation(); // Megakadályozza a tab kiválasztását, amikor a X-re kattintasz
            closeTab(file.id);
        };
        tab.appendChild(closeButton);

        tabs.appendChild(tab);
    }
}

function closeTab(fileId) {
    openFiles = openFiles.filter(f => f.id !== fileId);
    saveOpenFilesToLocalStorage();

    // Távolítsa el a tab elemet
    const tab = document.querySelector(`.tab[data-id="${fileId}"]`);
    if (tab) {
        tab.remove();
    }

    // Törölje a modal tartalmát, ha a bezárt tab volt a megnyitva
    const contentDiv = document.getElementById(`content-${fileId}`);
    if (contentDiv && !openFiles.length) {
        document.getElementById('modal').classList.add('hidden');
        document.getElementById('modalContent').innerHTML = ''; // Kiüríti a modal tartalmát
    } else if (contentDiv) {
        contentDiv.remove();
    }
}

async function selectTab(file) {
    if (Number(openedFileId) !== Number(file.id)) {
        const tabs = document.getElementById('tabs');
        const allTabs = Array.from(tabs.querySelectorAll('.tab'));

        openedFileId = file.id;
        // Válasszuk ki a megfelelõ tabot
        allTabs.forEach(tab => {
            if (Number(tab.dataset.id) === Number(file.id)) {
                tab.classList.add('active');
            } else {
                tab.classList.remove('active');
            }
        });
        await loadContent(file.id);

        // set the hidden inputs data for method creation
        document.getElementById('fileId').value = file.id;
        const methodId = document.getElementById('methodId');
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
    document.getElementById('tabs').innerHTML = ''; // Kiüríti a tabokat
    document.getElementById('modalContent').innerHTML = ''; // Kiüríti a modal tartalmát
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
