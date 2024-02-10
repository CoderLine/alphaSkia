const terser = require('@rollup/plugin-terser').default;
const dts = require('rollup-plugin-dts').default;

const commonOutput = {
    name: 'alphaskia'
};

const isWatch = process.env.ROLLUP_WATCH;
module.exports = [
    {
        input: `dist/lib/alphaskia.js`,
        output: [
            {
                file: 'dist/alphaskia.js',
                format: 'umd'
            },
            {
                file: 'dist/alphaskia.mjs',
                format: 'es'
            },
            {
                file: 'dist/alphaskia.min.mjs',
                format: 'es',
                plugins: [terser()]
            },
            {
                file: 'dist/alphaskia.min.js',
                format: 'umd',
                plugins: [terser()]
            }
        ].map(o => ({ ...commonOutput, ...o })),
        external: ['url', 'path', 'fs', 'node:module'],
        watch: {
            include: 'dist/lib/**',
            exclude: 'node_modules/**'
        }
    },
    {
        input: 'dist/types/alphaskia.d.ts',
        output: [
            {
                file: 'dist/alphaskia.d.ts',
                format: 'es'
            }
        ],
        external: ['url', 'path', 'fs', 'node:module'],
        plugins: [
            dts()
        ]
    }
];