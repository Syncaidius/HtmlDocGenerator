var objTypes = ["Class", "Struct", "Enum", "Interface"];

function populateIndex() {
    let di = $("#doc-index");
    di.html("");

    let keys = Object.keys(docData.Members);
    keys = keys.sort(sortStrings);

    let treePath = [ di ];

    keys.forEach((key, index) => {
        let memList = docData.Members[key];
        if(memList.length > 0)
            buildTreeNode(di, key, "", memList[0], treePath);
    });
}

function buildTreeNode(el, title, parentPath, dataNode, treePath) {
    let curPath = `${parentPath}${(parentPath.length > 0 && title ? "." : "")}${title}`;
    if (dataNode.ObjectType == "Namespace")
        title = curPath;

    let html = `<div id="i-${curPath}" 
class="sec-namespace sec-namespace${(treePath.length > 1 ? "-noleft" : "")}">`;
    html += `       <span class="namespace-toggle\">${title}</span><br/>`;
    html += `   <div id="in-${curPath}" class="sec-namespace-inner">`;
    html += `</div></div>`;
    el.append(html);

    let elInner = $(`#in-${curPath}`);
    console.log(`Current path '${curPath}'`);

    let keys = Object.keys(dataNode.Members); 
    let nextTreePath = [...treePath, elInner];

    keys = keys.sort(sortStrings);
    keys.forEach((mName, index) => {
        let memberArray = dataNode.Members[mName];
        if (memberArray.length == 0)
            return;



        // We're building an index tree, so we only need to know about the first entry of each member, to avoid duplicate index listings.
        let memberNode = memberArray[0];
        let memType = memberNode.ObjectType;

        switch (memType) {
            case "Namespace":
                let resetTreePath = [nextTreePath[0]]; // Go back to root of path
                buildTreeNode(nextTreePath[0], mName, curPath, memberNode, resetTreePath);
                break;

            case "Struct":
            case "Event":
            case "Constructor":
            case "Property":
            case "Class":
                // TODO generate sub-category using a "language" list for keywords (e.g. Class object type gets put into a "Classes" node)
                break;
        }
    });
}

function toHtml(str) {
    return str.replace('<', '&lt;').replace('>', '&gt;');
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
