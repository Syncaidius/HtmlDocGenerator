function populateIndex() {
    let di = $("#doc-index");
    di.html("");

    let keys = Object.keys(docData.Namespaces);
    keys = keys.sort();

    keys.forEach((ns, index) => {
        let contentHtml = "";
        let branchHtml = populateIndexBranch(ns, ns, contentHtml);
        di.append(branchHtml);
        console.log(`Added namespace '${ns}' to index`);
    });
}

function populateIndexBranch(namespace, title, contentHtml, depth = 0) {
    let html = `<div id=\"${namespace}${title}\" class=\"sec-namespace sec-namespace${(depth > 0 ? " - noleft" : "")}\">`;
    html += `<span class=\"namespace-toggle\">${title}</span><br/>`;
    html += `    <div class=\"sec-namespace-inner\">`;
    html += contentHtml;
    html += `</div></div>`;

    return html;
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
