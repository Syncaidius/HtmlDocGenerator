
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
            

        this.loadContent(elPage, dataNode, docPath);
        registerDocTargets(elPage);
    }

    loadContent(elPage, dataNode, docPath) { }

    getPathParts(docPath,) {
        return docPath.split(".");
    }

    getDocTarget(targetPath, memberName, title, extraClasses = null) {
        return `<a class="doc-target ${(extraClasses || "")}" data-target="${targetPath}" data-target-sec="${memberName}">${title}</a>`;
    }

    buildMemberSection(elPage, dataNode, docPath, docTypeFilter) {
        if (dataNode.Members == null || dataNode.Members.length == 0)
            return;

        let title = toPlural(docTypeFilter);
        let filtered = this.filterMembers(dataNode, docTypeFilter);
        if (filtered.length > 0) {
            // TODO support method/constructor/delegate doc types

            let memberHtml = "";

            filtered.forEach((mName, index) => {
                let memberVariants = dataNode.Members[mName];
                for (let i = 0; i < memberVariants.length; i++) {
                    let member = memberVariants[i];
                    let targetPath = `${docPath}.${mName}`;
                    let summary = member.Summary != null ? member.Summary : "&nbsp;";
                    let icon = getIcon(member);

                    let docTarget = this.getDocTarget(targetPath, mName, mName);
                    if (member.DocType == "Method")
                        docTarget += this.buildParameterHtml(member);

                    memberHtml += `
                    <tr>
                        <td>${icon}</td>
                        <td><span>${docTarget}</span></td>
                        <td>${summary}</td>
                    </tr>
                `;
                }
            });


            elPage.append(`
                <div class="obj-section">
                    <h2>${title}</h2>
                    <table>
                        <thead>
                            <tr>
                            <tr>
                                <th width="20px">&nbsp</th>
                                <th width="30%">Name</th>
                                <th>Summary</th>
                            </tr>
                        </thead>
                        <tbody>${memberHtml}</tbody>
                    </table>
                </div>
                `);
        }
    }

    buildParameterHtml(dataNode) {
        if (dataNode.Parameters == null || dataNode.Parameters.length == 0)
            return "()";

        let html = "";
        dataNode.Parameters.forEach((pNode, index) => {
            let pType = this.getPathParts(pNode.TypeName);
            let pTitle = pType[pType.length - 1];

            if (index > 0)
                html += ", ";
            html += this.getDocTarget(pNode.TypeName, pNode.Name, pTitle, "doc-parameter");
        });

        return `(${html})`;
    }

    filterMembers(dataNode, docTypeFilter) {
        let memberNames = Object.keys(dataNode.Members);
        let filtered = [];

        memberNames.forEach((mName, index) => {
            let variants = dataNode.Members[mName];
            if (variants.length == 0)
                return;

            if (variants[0].DocType == docTypeFilter)
                filtered.push(mName);

        });

        return filtered;
    }
}