function openCodeWindow(code) {
    var newWindow = window.open("", "_blank");
    newWindow.document.write("<pre>" + code + "</pre>");
}

function showCreateMethodForm() {
    document.getElementById('createMethodFlyIn').classList.remove('hidden');
}

function closeCreateMethodForm() {
    document.getElementById('createMethodFlyIn').classList.add('hidden');
}
let descriptionCount = 0;
function addDescriptionPoint() {
    const container = document.getElementById('descriptionContainer');
    const newPoint = document.createElement('div');
    newPoint.classList.add('description-item', 'mb-2');
    newPoint.innerHTML = '<input type="text" class="w-full p-2 border rounded-md" placeholder="Enter description point">';
    container.appendChild(newPoint);
}

function removeDescriptionField(id) {
    const descriptionDiv = document.getElementById(`description_${id}`).parentNode;
    descriptionDiv.remove();
}
async function createMethod() {
    const methodName = document.getElementById('methodName').value;
    const status = document.getElementById('newMethodStatus').value;
    const methodCode = document.getElementById('methodCode').value;
    const fileId = document.getElementById('fileId').value;
    const token = document.getElementById('antiForgeryToken').value;

    // Leírások összegyűjtése
    const descriptionItems = document.querySelectorAll('.description-item input');
    const descriptions = Array.from(descriptionItems).map(item => item.value);

    try {
        const response = await fetch('/ClassTree/CreateMethod', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({
                name: methodName,
                descriptions: descriptions,
                status: status,
                code: methodCode,
                fileId: fileId
            })
        });

        if (response.ok) {
            location.reload();
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
