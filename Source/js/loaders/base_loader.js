
class BaseLoader {
    /* elPageName = The name of the element that will contain help page content.
     * title = The title of the loaded page
     * dataNode = The docData node that contains the information we need to display on the page.
     * docPath = The data path we used to reach the current dataNode. 
     */
    load(elPageName, title, dataNode, docPath) {
        let elPage = $(`#${elPageName}`);
        if (elPage == null) {
            console.error(`Page element '${elPageName}' not found`);
            return;
        }

        elPage.html("");

        let pathParts = this.getPathParts(docPath);
        let lastPart = pathParts.length - 1;
        let pathHtml = "";
        let curPath = "";

        pathParts.forEach((part, index) => {
            if (index > 0) {
                pathHtml += ".";
                curPath += ".";
            }

            curPath += part;

            if (index < lastPart) {
                pathHtml += this.getDocTarget(curPath, "", part);
            } else {
                pathHtml += `<b>${title}</b>`;
            };
        });

        let iconHtml = getIcon(dataNode);
        elPage.append(`
            <div class="page-header">
                <div class="page-title">${iconHtml}<span id="page-title-span">${pathHtml}</span></div>
                <div class="page-type">${dataNode.DocType}</div>
            </div>
        `);
            

        let contentHtml = this.loadContent(dataNode, docPath);
        if (contentHtml != null && contentHtml.length > 0)
            elPage.append(`<div>${contentHtml}</div>`);

        registerDocTargets(elPage);
    }

    loadContent(dataNode, docPath) {
        return "";
    }

    getPathParts(docPath,) {
        return docPath.split(".");
    }

    getDocTarget(targetPath, memberName, title) {
        return `<a class="doc-target plain" data-target="${targetPath}" data-target-sec="${memberName}">${title}</a>`;
    }
}