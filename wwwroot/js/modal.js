let openFiles = [];
let openedFileId = -1;

document.addEventListener('DOMContentLoaded', async function () {
    const savedFiles = JSON.parse(localStorage.getItem('openFiles') || '[]');
    const openedFileId = localStorage.getItem('openedFileId')
    if (savedFiles.length > 0) {
        //document.getElementById('minimizedModalButton').classList.remove('hidden');
    }
    for (const savedFile of savedFiles) {
        if (!openFiles.some(file => file.id === savedFile.id)) {
            await createTab(savedFile);
        }
        if (savedFile.id === openedFileId) {
            await selectTab(savedFile);
        }
    }

    // Event listenerek hozzáadása a file linkekhez
    document.querySelectorAll('.file-item').forEach(link => {
        link.addEventListener('click',async function (e) {
            e.preventDefault();
            const fileId = this.getAttribute('data-file-id');
            const fileName = this.getAttribute('data-file-name');
            const isEntity = this.getAttribute('data-is-entity');
            const isMapping = this.getAttribute('data-is-mapping');

            const isEntityBool = isEntity === "true" || isEntity === "True"; 
            const isMappingBool = isMapping === "true" || isMapping === "True"; 

            await openFile({ id: fileId, name: fileName, isEntity: isEntityBool, isMapping: isMappingBool });
        });
    });
});
async function fetchFileContent(fileId, isEntity, isMapping) {
    try {
        //console.log(`/ClassTree/GetFileContent?fileId=${fileId}&isEntity=${isEntity}&isMapping=${isMapping}`)
        const response = await fetch(`/ClassTree/GetFileContent?fileId=${fileId}&isEntity=${isEntity}&isMapping=${isMapping}`);
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
function saveOpenedFileIdToLocalStorage(fileId) {
    localStorage.setItem('openedFileId', fileId);
}
async function openFile(file) {
    await createTab(file);
    selectTab(file);
    // MODAL MEGNYITÁSA
    document.getElementById('modal').classList.remove('hidden');
}
async function createTab(file) {
    if (!openFiles.some(f => f.id === file.id)) {
        openFiles.push(file);
        saveOpenFilesToLocalStorage();

        const tabs = document.getElementById('tabs');
        const tab = document.createElement('div');
        tab.className = 'file-tab';
        tab.dataset.id = file.id;
        tab.onclick = async () => await selectTab(file);

        const tabName = document.createElement('span');
        tabName.innerText = file.name;
        
        tab.appendChild(tabName);

        const closeButton = document.createElement('button');
        closeButton.innerText = String.fromCharCode('10005');
        closeButton.className = 'file-tab-close';
        closeButton.onclick = (e) => {
            e.stopPropagation(); // Megakadályozza a tab kiválasztását, amikor a X-re kattintasz
            closeTab(file.id);
        };
        tab.appendChild(closeButton);

        tabs.appendChild(tab);
    }
    else { console.log("vanmárlétezik")}
}

function closeTab(fileId) {
    openFiles = openFiles.filter(f => f.id !== fileId);
    saveOpenFilesToLocalStorage();
    // Távolítsa el a tab elemet
    const tab = document.querySelector(`.file-tab[data-id="${fileId}"]`);
    if (tab) {
        tab.remove();
    }

    // Törölje a modal tartalmát, ha a bezárt tab volt a megnyitva
    const contentDiv = document.getElementById(`content-${fileId}`);
    if (contentDiv) {
        contentDiv.remove();
    }
}

async function selectTab(file) {
    if (Number(openedFileId) !== Number(file.id)) {
        const tabs = document.getElementById('tabs');
        const allTabs = Array.from(tabs.querySelectorAll('.file-tab'));

        openedFileId = file.id;
        saveOpenedFileIdToLocalStorage(file.id);
        // Válasszuk ki a megfelelõ tabot
        allTabs.forEach(tab => {
            if (Number(tab.dataset.id) === Number(file.id)) {
                tab.classList.add('active-tab');
            } else {
                tab.classList.remove('active-tab');
            }
        });
        await loadContent(file.id, file.isEntity, file.isMapping);
        // set the hidden inputs data for method creation
        document.getElementById('fileId').value = file.id;
        const methodId = document.getElementById('methodId');
    }
}
async function loadContent(fileId, isEntity = false, isMapping = false) {    
    // fetch the data
    //console.log(fileId)
    const data = await fetchFileContent(fileId, isEntity, isMapping);
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
