const openFiles = [];

function openModal(fileContent) {
    const modal = document.getElementById('modal');
    const modalContent = document.getElementById('modalContent');
    const tabs = document.getElementById('tabs');

    const fileId = fileContent.fileId;
    const fileName = fileContent.fileName;

    // Check if the file is already open
    const existingTab = openFiles.find(file => file.id === fileId);
    if (existingTab) {
        modal.classList.remove('hidden');
        selectTab(existingTab.id);
        return;
    }

    // Create new tab and content
    openFiles.push({ id: fileId, name: fileName });
    const tab = document.createElement('div');
    tab.className = 'tab p-2 bg-gray-800 text-white rounded';
    tab.innerText = fileName;
    tab.onclick = () => selectTab(fileId);
    tabs.appendChild(tab);

    const contentDiv = document.createElement('div');
    contentDiv.id = `content-${fileId}`;
    contentDiv.className = `text-black hidden`;
    contentDiv.innerHTML = fileContent.html;
    modalContent.appendChild(contentDiv);

    // Show modal and select new tab
    modal.classList.remove('hidden');
    selectTab(fileId);
}

function selectTab(fileId) {
    // const modal = document.getElementById('modal');
    // if (modal.classList.contains("hidden") { 
    //     modal.classList.remove("hidden");
    // }
    const modalContent = document.getElementById('modalContent').children;
    for (let content of modalContent) {
        content.classList.add('hidden');
    }
    document.getElementById(`content-${fileId}`).classList.remove('hidden');
}

function closeModal() {
    openFiles.length = 0;
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

async function fetchFileContent(fileId, fileName) {
    try {
        const response = await fetch(`/ClassTree/GetFileContent?fileId=${fileId}`);
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        const data = await response.text();
        openModal({ fileId, fileName, html: data });
    } catch (error) {
        console.error('There was a problem with the fetch operation:', error);
    }
}

document.addEventListener('DOMContentLoaded', function () {
    // Add event listeners to file links
    document.querySelectorAll('.file-name').forEach(link => {
        link.addEventListener('click', function (e) {
            e.preventDefault();
            const fileId = this.getAttribute('data-file-id');
            const fileName = this.getAttribute('data-file-name');

            fetchFileContent(fileId, fileName);
        });
    });
});