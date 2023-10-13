import * as url from 'url';
import * as path from 'path';
import * as fs from 'fs';

const __dirname = url.fileURLToPath(new URL('.', import.meta.url));

const platformMap: any = {
    "win32": "win",
    "darwin": "macos"
};
const platform = platformMap[process.platform] || process.platform;
const arch = process.arch;

let searchPaths = [
    // working directory
    path.resolve('.', 'lib'),
    // script file
    path.resolve(__dirname, 'lib')
];

const nodeModulesIndex = __dirname.indexOf('node_modules');
if (nodeModulesIndex > 0) {
    searchPaths.push(path.join(__dirname.substring(0, nodeModulesIndex), 'node_modules', '@coderline', `alphaskia-${platform}`, 'lib'))
}

export function addSearchPaths(...paths: string[]) {
    searchPaths.push(...paths);
}

export function findAddonPath(): string | undefined {
    const libDirectory = `libalphaskianode-${platform}-${arch}-node`;
    for (const searchPath of searchPaths) {
        const addonPath = path.join(searchPath, libDirectory, 'libalphaskianode.node');
        if (fs.existsSync(addonPath)) {
            return addonPath;
        }
    }

    return undefined;
}