async function deleteFolderOrFile(type, id) {
    if (confirm(`Are you sure you want to delete this ${type}?`)) {
        try {
            const token = document.getElementById('antiForgeryToken').value;
            const response = await fetch('/ClassTree/DeleteFolderOrFile', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({ type: type, itemId: id })
            });

            if (response.ok) {
                console.log("OK!");
                // location.reload();
            } else {
                const errorText = await response.text();
                alert(`Failed to delete folder: ${errorText}`);
            }
        } catch (error) {
            console.error('Error deleting folder:', error);
            alert('An error occurred while trying to delete the folder.');
        }
    }
}
const toggleRenameInput = (type, item) => {
    const nameDiv = item.querySelector(`.${type}-name`);
    const nameInput = item.querySelector(`.${type}-name-input`);
    const saveButton = item.querySelector('.save-btn');
    const cancelButton = item.querySelector('.cancel-btn');

    nameDiv.classList.toggle('hidden');
    nameInput.classList.toggle('hidden');
    saveButton.classList.toggle('hidden');
    cancelButton.classList.toggle('hidden');
}
function editFolderOrFile(type, folderId) {
    const item = document.querySelector(`.${type}-item[data-${type}-id='${folderId}']`);
    const nameDiv = item.querySelector(`.${type}-name`);
    const nameInput = item.querySelector(`.${type}-name-input`);

    const saveButton = item.querySelector('.save-btn');
    const cancelButton = item.querySelector('.cancel-btn');

    toggleRenameInput(type, item);

    saveButton.onclick = (event) => {
        event.stopPropagation();
        saveName(type, folderId, nameInput.value.trim());
    };
    cancelButton.onclick = (event) => {
        event.stopPropagation();
        toggleRenameInput(type, item);
        nameInput.value = nameDiv.innerText.trim();
    };
}

async function saveName(type, folderId, newFolderName) {
    try {
        const token = document.getElementById('antiForgeryToken').value;
        const response = await fetch('/ClassTree/RenameFolderOrFile', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({ type: type, itemId: folderId, newName: newFolderName })
        });

        if (response.ok) {
            location.reload();
        } else {
            const errorText = await response.text();
            alert(`Failed to rename ${type}: ${errorText}`);
        }
    } catch (error) {
        console.error(`Error renaming ${type}:`, error);
        alert(`An error occurred while trying to rename the ${type}.`);
    }
}
async function deleteFolder(folderId) {
    if (confirm('Are you sure you want to delete this folder?')) {
        console.log(folderId)
        try {
            const token = document.getElementById('antiForgeryToken').value;
            const response = await fetch('/ClassTree/DeleteFolder', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({ folderId: folderId })
            });

            if (response.ok) {
                 location.reload();
            } else {
                const errorText = await response.text();
                alert(`Failed to delete folder: ${errorText}`);
            }
        } catch (error) {
            console.error('Error deleting folder:', error);
            alert('An error occurred while trying to delete the folder.');
        }
    }
}
function editFolder(folderId) {
    const folderItem = document.querySelector(`.folder-item[data-folder-id='${folderId}']`);
    const folderNameDiv = folderItem.querySelector('.folder-name');
    const folderNameInput = folderItem.querySelector('.folder-name-input');
    const saveButton = folderItem.querySelector('.save-btn');
    const cancelButton = folderItem.querySelector('.cancel-btn');

    folderNameDiv.classList.add('hidden');
    folderNameInput.classList.remove('hidden');
    saveButton.classList.remove('hidden');
    cancelButton.classList.remove('hidden');

    saveButton.onclick = (event) => {
        event.stopPropagation();
        // saveFolderName(folderId, folderNameInput.value.trim());
    };
    cancelButton.onclick = (event) => {
        event.stopPropagation();
        // cancelEdit(folderId, folderNameDiv.innerText.trim());
    };
}
async function createFolderOrFile(type) {
    try {
        const token = document.getElementById('antiForgeryToken').value;
        const name = type === 'folder' ? document.getElementById('newFolderName').value : document.getElementById('newFileName').value;
        const status = type === 'folder' ? document.getElementById('newFolderStatus').value : document.getElementById('newFileStatus').value;
        const parentId = document.getElementById('parentId').value;
        const response = await fetch('/ClassTree/CreateFolderOrFile', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({ type: type, name: name, status: status, parentId: parentId })
        });

        if (response.ok) {
            location.reload();
        } else {
            const errorText = await response.text();
            alert(`Failed to create ${type}: ${errorText}`);
        }
    } catch (error) {
        console.error(`Error creating ${type}:`, error);
        alert(`An error occurred while trying to create the ${type}.`);
    }
}