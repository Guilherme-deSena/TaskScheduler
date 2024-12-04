const uri = 'api/Tasks';
let tasks = [];

const now = new Date();
const localISOTime = new Date(now.getTime() - now.getTimezoneOffset() * 60000)
    .toISOString()
    .slice(0, 16);

document.getElementById("add-start").setAttribute("min", localISOTime);
document.getElementById("add-end").setAttribute("min", localISOTime);

document.getElementById("add-start").setAttribute("value", localISOTime);
document.getElementById("add-end").setAttribute("value", localISOTime);

document.getElementById("add-start").addEventListener("change", function () {
    let start = this;
    let end = document.getElementById("add-end");
    if (start.value > end.value)
        end.value = start.value;
});
document.getElementById("add-end").addEventListener("change", function () {
    let start = document.getElementById("add-start");
    let end = this;
    if (start.value > end.value)
        start.value = end.value;
});
document.getElementById("edit-start").addEventListener("change", function () {
    let start = this;
    let end = document.getElementById("edit-end");
    if (start.value > end.value)
        end.value = start.value;
});
document.getElementById("edit-end").addEventListener("change", function () {
    let start = document.getElementById("edit-start");
    let end = this;
    if (start.value > end.value)
        start.value = end.value;
});

function getItems() {
    fetch(uri)
        .then(response => response.json())
        .then(data => _displayItems(data))
        .catch(error => console.error('Unable to get items.', error));
}

function addItem() {
    const nameTextbox = document.getElementById('add-name');
    const startDateTextbox = document.getElementById('add-start');
    const endDateTextbox = document.getElementById('add-end');
    const priorityTextbox = document.getElementById('add-priority');
    const feedbackText = document.getElementById('feedback-add');

    if (nameTextbox.value == null || nameTextbox.value == '') {
        feedbackText.textContent = 'Your task must have a name.';
        return;
    }
    if (startDateTextbox.value == null) {
        feedbackText.textContent = 'Your task must have a starting date.';
        return;
    }
    if (endDateTextbox.value == null) {
        feedbackText.textContent = 'Your task must have an ending date.';
        return;
    }
    if (priorityTextbox.value == null || priorityTextbox.value == '') {
        feedbackText.textContent = 'Your task must have a priority.';
        return;
    }

    const item = {
        Name: nameTextbox.value.trim(),
        startDate: startDateTextbox.value,
        endDate: endDateTextbox.value,
        priority: priorityTextbox.value,
        isCOmplete: false
    };

    fetch(uri, {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(item)
    })
        .then(response => response.json())
        .then(() => {
            getItems();
            nameTextbox.value = '';
            priorityTextbox.value = '';
        })
        .catch(error => console.error('Unable to add item.', error));

    feedbackText.textContent = `Task "${item.Name}" was added.`;
}

function deleteItem(id) {
    fetch(`${uri}/${id}`, {
        method: 'DELETE'
    })
        .then(() => getItems())
        .catch(error => console.error('Unable to delete item.', error));
}

function displayEditForm(id) {
    const item = tasks.find(item => item.id === id);
    const timeZoneOffset = new Date().getTimezoneOffset() * 60000;

    document.getElementById('edit-name').value = item.Name;
    document.getElementById('edit-id').value = item.id;
    document.getElementById('edit-isCompleted').checked = item.isCompleted;

    let date = new Date(item.startDate).getTime();
    let timeZoned = new Date(date - timeZoneOffset);
    document.getElementById('edit-start').value = timeZoned.toISOString().slice(0, 16);

    date = new Date(item.endDate).getTime();
    timeZoned = new Date(date - timeZoneOffset);
    document.getElementById('edit-end').value = timeZoned.toISOString().slice(0, 16);

    document.getElementById('edit-priority').value = item.priority;
    document.getElementById('editForm').style.display = 'block';
    document.getElementById('feedback-edit').textContent = '';
}

function updateItem() {
    const item = {
        id: document.getElementById('edit-id').value,
        Name: document.getElementById('edit-name').value.trim(),
        startDate: document.getElementById('edit-start').value,
        endDate: document.getElementById('edit-end').value,
        priority: document.getElementById('edit-priority').value,
        isCompleted: document.getElementById('edit-isCompleted').checked
    };

    const feedbackText = document.getElementById('feedback-edit');
    if (item.Name == null || item.Name == '') {
        feedbackText.textContent = 'Your task must have a name.';
        return;
    }
    if (item.startDate == null) {
        feedbackText.textContent = 'Your task must have a starting date.';
        return;
    }
    if (item.endDate == null) {
        feedbackText.textContent = 'Your task must have an ending date.';
        return;
    }
    if (item.priority == null || item.priority == '') {
        feedbackText.textContent = 'Your task must have a priority.';
        return;
    }

    fetch(`${uri}/${item.id}`, {
        method: 'PUT',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(item)
    })
        .then(() => getItems())
        .catch(error => console.error('Unable to update item.', error));

    closeInput();

    return false;
}

function closeInput() {
    document.getElementById('editForm').style.display = 'none';
}

function _displayCount(itemCount) {
    const name = (itemCount === 1) ? 'task' : 'tasks';

    document.getElementById('counter').innerText = `${itemCount} ${name} in total`;
}

function _displayItems(data) {
    const tBody = document.getElementById('tasks');
    tBody.innerHTML = '';

    _displayCount(data.length);

    const button = document.createElement('button');

    data.forEach(item => {
        let isCompleteCheckbox = document.createElement('input');
        isCompleteCheckbox.type = 'checkbox';
        isCompleteCheckbox.disabled = true;
        isCompleteCheckbox.checked = item.isCompleted;

        let editButton = button.cloneNode(false);
        editButton.innerText = 'Edit';
        editButton.setAttribute('onclick', `displayEditForm('${item.id}')`);

        let deleteButton = button.cloneNode(false);
        deleteButton.innerText = 'Delete';
        deleteButton.setAttribute('onclick', `deleteItem('${item.id}')`);

        let tr = tBody.insertRow();

        let td0 = tr.insertCell(0);
        td0.appendChild(isCompleteCheckbox);

        let td1 = tr.insertCell(1);
        let textNode = document.createTextNode(item.Name);
        td1.appendChild(textNode);

        let td2 = tr.insertCell(2);
        let date = new Date(item.startDate);
        textNode = document.createTextNode(new Date(date - date.getTimezoneOffset() * 60000).toISOString().slice(0, 16));
        td2.appendChild(textNode);

        let td3 = tr.insertCell(3);
        date = new Date(item.endDate);
        textNode = document.createTextNode(new Date(date - date.getTimezoneOffset() * 60000).toISOString().slice(0, 16));
        td3.appendChild(textNode);

        let td4 = tr.insertCell(4);
        textNode = document.createTextNode(item.priority);
        td4.appendChild(textNode);

        let td5 = tr.insertCell(5);
        td5.appendChild(editButton);

        let td6 = tr.insertCell(6);
        td6.appendChild(deleteButton);
    });

    tasks = data;
}