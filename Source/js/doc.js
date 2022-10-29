var objTypes = ["Class", "Struct", "Enum", "Interface"];

function populateIndex() {
    let di = $("#doc-index");
    di.html("");

    let keys = Object.keys(docData.Members);
    keys = keys.sort(sortStrings);

    let treePath = [di];

    keys.forEach((key, index) => {
        let memList = docData.Members[key];
        if (memList.length > 0)
            buildTreeNode(di, key, "", memList[0], treePath);
    });
}

function buildTreeNode(el, title, parentPath, dataNode, treePath, empty = false) {
    let curPath = `${parentPath}${(parentPath.length > 0 && title ? "." : "")}${title}`;
    if (dataNode.DocType == "Namespace")
        title = curPath;

    let idName = toIDName(curPath);
    el.append(`<div id="i-${idName}" class="sec-namespace${(treePath.length > 1 ? "-noleft" : "")}">
                    <span class="namespace-toggle\">${title}</span><br/>
                    <div id="in-${idName}" class="sec-namespace-inner"></div>
                </div>`);

    console.log(`Current path '${curPath}'`);
    let elInner = $(`#in-${idName}`);
    if (elInner.length > 0)
        console.log(`Created element ${elInner[0].id}`);
    else
        console.log(`Unable to find element in-${idName}`);

    console.log(`Added element ${elInner[0].id} to ${el[0].id}`);

    if (!dataNode.Members || empty == true)
        return elInner;

    let keys = Object.keys(dataNode.Members);
    let nextTreePath = [...treePath, elInner];

    keys = keys.sort(sortStrings);
    keys.forEach((mName, index) => {
        let memberArray = dataNode.Members[mName];
        if (memberArray.length == 0)
            return;

        // We're building an index tree, so we only need to know about the first entry of each member, to avoid duplicate index listings.
        let memberNode = memberArray[0];
        let memType = memberNode.DocType;

        switch (memType) {
            case "Namespace":
                let resetTreePath = [nextTreePath[0]]; // Go back to root of path
                buildTreeNode(nextTreePath[0], mName, curPath, memberNode, resetTreePath);
                break;

            case "Struct":
            case "Interface":
            case "Class":
            case "Enum":
                let categoryName = toPlural(memType);
                buildCategorizedNode(elInner, categoryName, mName, curPath, memberNode, nextTreePath);
                break;
        }
    });

    return elInner;
}

function buildCategorizedNode(elParent, category, title, parentPath, dataNode, treePath) {
    elParent.categories = elParent.categories || {};

    if (elParent.categories[category] == null)
        elParent.categories[category] = buildTreeNode(elParent, category, parentPath, dataNode, treePath, true);

    let elInner = elParent.categories[category];
    let nextTreePath = [...treePath, elInner];
    let curPath = `${parentPath}${(parentPath.length > 0 && category ? "." : "")}${category}`;
    buildTreeNode(elInner, title, curPath, dataNode, nextTreePath);
}

function toPlural(word) {
    if (word.length == 0)
        return word;

    let last = word[word.length - 1].toLowerCase();
    switch (last) {
        case 'y':
            return word.substring(0, word.length) + "ies";

        case 'h':
        case 's':
        case 'x':
        case 'z':
            return word.substring(0, word.length) + "es";
    }

    return word + "s";
}

function toIDName(str) {
    return str.replace(/(\.|>|<|,| )/g, '-');
}

function toHtml(str) {
    return str.replace(/</g, '&lt;').replace(/>/g, '&gt;');
}

function getIcon(el) {
    return `<img src="img/${el.DocType.toLowerCase()}.png"/>`;
}

function sortStrings(a, b) {
    if (a > b)
        return 1;

    if (a < b)
        return -1;

    return 0;
}

$(document).ready(function () {
    // Set page title
    $('#doc-title').html(docData.Name);
    $('#doc-intro').html(docData.Intro);

    populateIndex();

    let toggler = document.getElementsByClassName("namespace-toggle");
    let i;

    for (i = 0; i < toggler.length; i++) {
        toggler[i].addEventListener("click", function () {
            {
                this.parentElement.querySelector(".sec-namespace-inner").classList.toggle("sec-active");
                this.classList.toggle("namespace-toggle-down");
            }
        });
    }

    let pageTargets = document.getElementsByClassName("doc-page-target");
    for (i = 0; i < pageTargets.length; i++) {
        {
            pageTargets[i].addEventListener("click", function (e) {
                {
                    //document.getElementById('content-target').src = e.target.dataset.url
                    // TODO set main content to use whichever loader we need (object or member).
                }
            });
        }
    }
})
