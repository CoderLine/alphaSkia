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
                file: 'dist/alphaskia.mjs',
                format: 'es'
            },
            {
                file: 'dist/alphaskia.min.mjs',
                format: 'es',
                plugins: [terser()]
            }
        ].map(o => ({ ...commonOutput, ...o })),
        external: [],
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
        plugins: [
            dts()
        ]
    }
];