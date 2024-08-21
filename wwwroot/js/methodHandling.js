function openCodeWindow(code) {
    var newWindow = window.open("", "_blank");
    newWindow.document.write("<pre>" + code + "</pre>");
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

            if (response.ok) {
                console.log("ok");
                //location.reload();
            } else {
                const errorText = await response.text();
                alert(`Failed to delete method: ${errorText}`);
            }
        } catch (error) {
            console.error('Error deleting method:', error);
            alert('An error occurred while trying to delete the method.');
        }
    }
}
function showCreateMethodForm() {
    document.getElementById('modalTitle').innerText = "Create New Method";
    document.getElementById('saveMethodButton').innerText = "Create";
    document.getElementById('methodId').value = "";
    document.getElementById('methodName').value = "";
    document.getElementById('newMethodStatus').value = "New";
    document.getElementById('methodCode').value = "";
    document.getElementById('description').value = "";
    document.getElementById('createMethodFlyIn').classList.remove('hidden');
}
function showEditMethodForm(id, name, status, code, description) {
    document.getElementById('modalTitle').innerText = "Edit Method";
    document.getElementById('saveMethodButton').innerText = "Save";
    document.getElementById('methodId').value = id;
    document.getElementById('methodName').value = name;
    document.getElementById('newMethodStatus').value = status;
    document.getElementById('methodCode').value = code;
    document.getElementById('description').value = description;
    document.getElementById('createMethodFlyIn').classList.remove('hidden');
}
function closeCreateMethodForm() {
    document.getElementById('createMethodFlyIn').classList.add('hidden');
}
async function createMethod() {
    const methodId = document.getElementById('methodId').value;
    const methodName = document.getElementById('methodName').value;
    const status = document.getElementById('newMethodStatus').value;
    const description = document.getElementById('description').value;
    const methodCode = document.getElementById('methodCode').value;
    const fileId = document.getElementById('fileId').value;
    const token = document.getElementById('antiForgeryToken').value;
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
                code: methodCode,
                fileId: fileId
            })
        });

        if (response.ok) {
            //location.reload();
            console.log("ok")
        } else {
            const errorText = await response.text();
            alert(`Failed to create method: ${errorText}`);
        }
    } catch (error) {
        console.error('Error creating method:', error);
        alert('An error occurred while trying to create the method.');
    } finally {
        closeCreateMethodForm(); // Close the fly-in after creating the method
    }
}
