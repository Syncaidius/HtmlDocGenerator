var objTypes = ["Class", "Struct", "Enum", "Interface"];

function populateIndex() {
    let di = $("#doc-index");
    di.html("");

    let keys = Object.keys(docData.Namespaces);
    keys = keys.sort();

    keys.forEach((ns, index) => {
        let branchHtml = populateIndexBranch(ns, ns, () => {
            let cHtml = "";
            for (let i = 0; i < objTypes.length; i++) { 
                let oType = objTypes[i];
                cHtml += generateObjectIndex(oType, docData.Namespaces[ns], oType, oType);
            }

        });
        di.append(branchHtml);
    });
}

function getIcon(el) {
    return `${el.DocType.toLowerCase()}.png`;
}

function populateIndexBranch(namespace, title, contentCallback, depth = 0) {
    let html = `<div id="${namespace}${title}" class="sec-namespace sec-namespace${(depth > 0 ? " - noleft" : "")}">`;
    html += `       <span class="namespace-toggle\">${title}</span><br/>`;
    html += `   <div class="sec-namespace-inner">`;

    if(contentCallback != null)
        html += contentCallback();

    html += `</div></div>`;

    return html;
}

function generateObjectIndex(title, ns, title, objType) {
    let filteredList = ns.Objects.filter(o => o.DocType == objType);
    if (filteredList.length == 0)
        return "";

    return populateIndexBranch(ns.Name, title, () => {
        let html = "";
        for (let obj of filteredList) {
            let htmlIcon = getIcon(obj);

            if (obj.MembersByType.length == 0 || obj.DocType == "Enum") {
                html += `<table class="sec-obj-index"><thead><tr>
                            <th class="col-type-icon"></th>
                            <th class="col-type-name"></th>
                            </tr></thead><tbody>
                            <tr id="{ns}-{obj.HtmlName}" class="sec-namespace-obj">
                                <td>${htmlIcon}</td>
                                <td><span class=\"doc-page-target\" data-url=\"{obj.HtmlUrl}\">{obj.HtmlName}</span></td>
                            </tr>
                        </tbody></table>`;
            }
            else {
                let nsObj = `${ns}${title}`;
                html += populateIndexBranch( nsObj, obj.HtmlName, () => {
                    let innerHtml = "";
                    // TODO replace with JS section generators
                    /*for(let secGen in _objSectionGens)
                    {
                        if (secGen is ObjectMemberSectionGenerator memSecGen)
                {
                                    string secHtml = memSecGen.GenerateIndexTreeItems(context, nsObj, obj);

                    if (secHtml.Length > 0) {
                                        string secTitle = secGen.GetTitle();
                                        string nsSec = $"{nsObj}{secTitle}";

                        innerHtml += GenerateTreeBranch(context, nsSec, secTitle, secHtml, 3);
                    }*/
                });
            }
        }

        return html;
    }, 2);
}

$(document).ready(function () {
    // Set page title
    $('#doc-title').html(docData.Title);
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
                    document.getElementById('content-target').src = e.target.dataset.url

                }
            });
        }
    }
})
