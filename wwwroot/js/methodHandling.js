function openCodeWindow(code) {
    var newWindow = window.open("", "_blank");
    var escapedCode = code
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#39;");
    newWindow.document.write("<pre>" + escapedCode + "</pre>");
}

async function deleteMethod(methodId) {
    if (confirm('Are you sure you want to delete this method?')) {
        try {
            const token = document.getElementById('antiForgeryToken').value;
            const response = await fetch(`/ClassTree/DeleteMethod/${methodId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
            });

            if (!response.ok) {
                const errorText = await response.text();
                alert(`Failed to delete method: ${errorText}`);
            }
        } catch (error) {
            console.error('Error deleting method:', error);
            alert('An error occurred while trying to delete the method.');
        }
    }
}
// Segédfüggvény a modal alapértelmezett értékeinek beállításához
function configureModal({ title, buttonText, methodId = '', name = '', status = 'new', description = '', code = '' }) {
    const modal = document.querySelector('#createMethodFlyIn');
    modal.classList.remove('hidden');
    console.log(status)
    document.querySelector('#modalTitle').innerText = title;
    document.querySelector('#saveMethodButton').innerText = buttonText;
    document.querySelector('#methodId').value = methodId;
    document.querySelector('#methodName').value = name;
    document.querySelector('#newMethodStatus').value = status.toLowerCase();
    document.querySelector('#description').value = description;
    document.querySelector('#methodCode').value = code;
}

// Függvény az új metódus létrehozására
function showCreateMethodForm() {
    configureModal({
        title: 'Create New Method',
        buttonText: 'Create',
    });
}

// Függvény egy meglévő metódus szerkesztésére
async function showEditMethodForm(id, name, status, description) {
    try {
        // AJAX hívás a metódus kódjának lekéréséhez
        const response = await fetch(`/ClassTree/GetMethodCode/${id}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error(`Failed to fetch method code: ${response.statusText}`);
        }

        // A válasz JSON-ként történő feldolgozása
        const data = await response.json();

        // Modal konfigurálása a lekért kóddal
        configureModal({
            title: 'Edit Method',
            buttonText: 'Save',
            methodId: id,
            name: name,
            status: status,
            description: description,
            code: data.code // A lekért metóduskód beállítása
        });
    } catch (error) {
        console.error('Error fetching method code:', error);
        alert('An error occurred while trying to fetch the method code.');
    }
}

// Modal bezárása
function closeCreateMethodForm() {
    document.querySelector('#createMethodFlyIn').classList.add('hidden');
}

// Metódus mentése
async function createMethod() {
    const methodId = document.querySelector('#methodId').value;
    const methodName = document.querySelector('#methodName').value;
    const status = document.querySelector('#newMethodStatus').value;
    const description = document.querySelector('#description').value;
    const code = document.querySelector('#methodCode').value;
    const fileId = document.querySelector('#fileId').value;
    const token = document.querySelector('#antiForgeryToken').value;

    const url = methodId ? `/ClassTree/EditMethod/${methodId}` : '/ClassTree/CreateMethod';
    const method = methodId ? 'PUT' : 'POST';

    try {
        const response = await fetch(url, {
            method: method,
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({
                name: methodName,
                description: description,
                status: status,
                code: code,
                fileId: fileId
            })
        });

        if (!response.ok) {
            const errorText = await response.text();
            alert(`Failed to save method: ${errorText}`);
            return;
        }
        location.reload()
    } catch (error) {
        console.error('Error saving method:', error);
        alert('An error occurred while trying to save the method.');
    } finally {
        closeCreateMethodForm();
    }
}

// Eseményfigyelők hozzáadása a gombokhoz
document.querySelector('#saveMethodButton').addEventListener('click', createMethod);
document.querySelector('#cancelMethodButton').addEventListener('click', closeCreateMethodForm);


function refreshContent() {
    const fileId = document.getElementById("fileId").value;
    loadContent(fileId);
}

async function editFileDesc(id) {
    const saveButton = document.getElementById("save-button");
    const cancelButton = document.getElementById("cancel-button");
    const descriptionText = document.getElementById("description-text");
    const editDescription = document.getElementById("edit-description");
    const descriptionInput = document.getElementById("description-input")

    descriptionText.style.display = "none";
    editDescription.style.display = "block";
    cancelButton.addEventListener("click", function () {
        editDescription.style.display = "none";
        descriptionText.style.display = "block";
    });
    saveButton.addEventListener("click", function () {
        const newDescription = descriptionInput.value;
        const fileId = descriptionInput.getAttribute('data-file-id');

        fetch('/classtree/UpdateFileDescription', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest',
            },
            body: JSON.stringify({ id: fileId, description: newDescription })
        })
        .then(response => {
            if (!response.ok) {
                // Ha nem OK, feldobunk egy hibát
                return response.json().then(error => {
                    throw new Error(error.error || "An error occurred");
                });
            }
            return response.json(); // JSON válasz feldolgozása
        })
        .then(data => {
            location.reload();
        })
        .catch(error => {
            console.error('Error:', error);
            alert(error.message);
        });
    });
};

