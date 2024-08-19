document.addEventListener('DOMContentLoaded', function () {
    console.log("haha")
});

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

function createMethod() {
    const methodName = document.getElementById('methodName').value;
    const methodDescription = document.getElementById('methodDescription').value;

    // Placeholder for the actual method creation logic
    console.log(`Creating method: ${methodName}, Description: ${methodDescription}`);

    closeCreateMethodForm(); // Close the fly-in after creating the method
}