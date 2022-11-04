
class MethodLoader extends BaseLoader {
    loadContent(elPage, dataNode, docPath) {

        let paramHtml = "";

        if (dataNode.Parameters == null || dataNode.Parameters.length == 0)
            return;

        dataNode.Parameters.forEach((pNode, index) => {
            let typeName = this.manager.getNode(pNode.TypeName);
            let typeTitle = this.manager.getPathTitle(pNode.TypeName);
            let typeTarget = "";

            if (typeName != null)
                typeTarget = this.getDocTarget(pNode.TypeName, "", typeTitle);
            else
                typeTarget = this.getDocInvalid(pNode.TypeName);

            let summary = pNode.Summary != null ? pNode.Summary : "&nbsp";
            let keywords = this.getParameterKeyword(pNode);

            paramHtml += `
            <tr>
                <td></td>
                <td>${keywords}${pNode.Name}</td>
                <td>${typeTarget}</td>
                <td>${summary}</td>
            </tr>`;
        });

        elPage.append(`
            <div class="obj-section">
                <h2>Parameters</h2>
                <table>
                    <thead>
                        <tr>
                        <tr>
                            <th width="20px">&nbsp</th>
                            <th width="30%">Name</th>
                            <th width="20%">Type</th>
                            <th>Summary</th>
                        </tr>
                    </thead>
                    <tbody>${paramHtml}</tbody>
                </table>
            </div>
        `);

        this.displayMethodCall(elPage, dataNode, docPath);
    }

    displayMethodCall(elPage, dataNode, docPath) {
        let returnCall = "";
        if (dataNode.ReturnTypeName != null) {
            let returnNode = this.manager.getNode(dataNode.ReturnTypeName);
            let returnTypeName = this.manager.getPathTitle(dataNode.ReturnTypeName);

            let returnTarget = this.getDocTarget(dataNode.ReturnTypeName, "", returnTypeName);
            returnCall = `${returnTarget} result = `;
        }

        let name = this.manager.getPathTitle(docPath);

        let parentPath = this.getParentPath(docPath);
        let parentName = this.manager.getPathTitle(parentPath);
        let parentNode = this.manager.getNode(parentPath);
        let parentTarget = this.getDocTarget(parentPath, "", parentName);

        let parameters = "";
        if (dataNode.Parameters != null && dataNode.Parameters.length > 0) {
            dataNode.Parameters.forEach((pNode, index) => {
                let pType = this.getPathParts(pNode.TypeName);
                let pTitle = pType[pType.length - 1];
                let keyword = this.getParameterKeyword(pNode);

                if (index > 0)
                    parameters += ", ";

                let docTarget = this.getDocTarget(pNode.TypeName, pNode.Name, pTitle, 0, "doc-parameter");
                parameters += `${keyword} ${docTarget} ${pNode.Name}`;
            });
        }

        elPage.append(`
            <div class="obj-section code">
                <h2>Call Example</h2>
                <table>
                    <tbody><tr><td>${returnCall}${parentTarget}.${name}(${parameters})</td></tr></tbody>
                </table>
            </div>
        `);
    }
}