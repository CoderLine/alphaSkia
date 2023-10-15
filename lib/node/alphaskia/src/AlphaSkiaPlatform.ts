import * as url from 'url';
import * as path from 'path';
import * as fs from 'fs';

const __dirname = url.fileURLToPath(new URL('.', import.meta.url));

const ridPlatformMap: any = {
    "win32": "win",
    "darwin": "macos"
};
const ridPlatform = ridPlatformMap[process.platform] || process.platform;

const packagePlatformMap: any = {
    "win32": "windows",
    "darwin": "macos"
};
const packagePlatform = packagePlatformMap[process.platform] || process.platform;
const arch = process.arch;

let searchPaths = [
    // working directory
    path.resolve('.', 'lib'),
    // script file
    path.resolve(__dirname, 'lib')
];

const nodeModulesIndex = __dirname.indexOf('node_modules');
if (nodeModulesIndex > 0) {
    searchPaths.push(path.join(__dirname.substring(0, nodeModulesIndex), 'node_modules', '@coderline', `alphaskia-${packagePlatform}`, 'lib'))
}

/**
 * Adds custom search paths which should be considered when loading the native addon of alphaSkia.
 * @param paths The paths to add.
 */
export function addSearchPaths(...paths: string[]) {
    searchPaths.push(...paths);
}

/**
 * Attempts to resolve the native node addon for alphaSkia which is typically placed in an operating system platform
 * and architecture specific folder starting from certain search paths.
 * 
 * By default the following paths are considered
 * * <working directory>/lib
 * * <path-to-alphaskia-script>/lib
 * * node_modules/@coderline/alphaskia-<operating system>/lib (if we detect that the alphaSkia script file is below node_modules).
 * @returns The resolved path to the native node addon to load for this platform or undefined if it could not be found.
 */
export function findAddonPath(): string | undefined {
    const libDirectory = `libalphaskianode-${ridPlatform}-${arch}-node`;
    for (const searchPath of searchPaths) {
        const addonPath = path.join(searchPath, libDirectory, 'libalphaskianode.node');
        if (fs.existsSync(addonPath)) {
            return addonPath;
        }
    }

    return undefined;
}