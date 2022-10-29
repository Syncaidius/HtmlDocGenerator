import { BaseLoader } from "./base_loader";

export class ObjectLoader extends BaseLoader {

    load(dataNode, docPath) {

    }

    loadSection(title, members, isMethods) {
        // TODO display table with "name" and "summary" columns
        // TODO sort alphabetically
        // TODO if isMethods is true, add () and parameter list to "name" column
    }
}