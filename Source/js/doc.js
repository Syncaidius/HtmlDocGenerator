var objTypes = ["Class", "Struct", "Enum", "Interface"];

function populateIndex() {
    let di = $("#doc-index");
    di.html("");

    let keys = Object.keys(docData.Namespaces);
    keys = keys.sort();

    keys.forEach((ns, index) => {
        let branchHtml = populateIndexBranch(ns, ns, 0, () => {
            let cHtml = "";
            for (let i = 0; i < objTypes.length; i++) { 
                let oType = objTypes[i];
                cHtml += generateObjectIndex(oType, docData.Namespaces[ns], oType, oType);
            }

            return cHtml;
        });
        di.append(branchHtml);
    });
}

function toHtml(str) {
    return str.replace('<', '&lt;').replace('>', '&gt;');
}

function getIcon(el) {
    return `<img src="img/${el.DocType.toLowerCase()}.png"/>`;
}

function populateIndexBranch(namespace, title, depth, contentCallback) {
    let html = `<div id="${namespace}${title}" class="sec-namespace sec-namespace${(depth > 0 ? `-noleft` : ``)}">`;
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

    return populateIndexBranch(ns, title, 1, () => {
        let html = "";
        for (let obj of filteredList) {
            let htmlIcon = getIcon(obj);
            let htmlName = toHtml(obj.Name);

            if (obj.MembersByType.length == 0 || obj.DocType == "Enum") {
                html += `<table class="sec-obj-index"><thead><tr>
                            <th class="col-type-icon"></th>
                            <th class="col-type-name"></th>
                            </tr></thead><tbody>
                            <tr id="${ns}-${htmlName}" class="sec-namespace-obj">
                                <td>${htmlIcon}</td>
                                <td><span class="doc-page-target" data-url="${obj.Name}">${htmlName}</span></td>
                            </tr>
                        </tbody></table>\n`;
            }
            else {
                //let nsObj = `${ns}${title}`;
                html += populateIndexBranch(ns, obj.HtmlName, 2, () => {
                    let innerHtml = "";
                    // TODO replace with JS section generators

                    /*for (let secGen in _objSectionGens) {
                        if (secGen is ObjectMemberSectionGenerator memSecGen)
                        {
                            let secHtml = memSecGen.GenerateIndexTreeItems(context, nsObj, obj);
    
                            if (secHtml.Length > 0) {
                                let secTitle = secGen.GetTitle();
                                let nsSec = $"{nsObj}{secTitle}";
    
                                innerHtml += GenerateTreeBranch(context, nsSec, secTitle, secHtml, 3);
                            }
                        }
                    }*/

                    return innerHtml;
                });
            }
        }

        return html;
    });
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
                    //document.getElementById('content-target').src = e.target.dataset.url
                    // TODO set main content to use whichever loader we need (object or member).
                }
            });
        }
    }
})
