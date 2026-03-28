import { createServer, resolveConfig } from 'vite';
import * as fs from 'node:fs';
import * as path from 'node:path';

async function generate() {
    const config = await resolveConfig({}, 'build');
    const SITE_URL = config.env.VITE_SITE_URL || '';
    const DIST_DIR = path.resolve(config.root, config.build.outDir || 'dist');

    console.log(`🤖 Start to generate llms docs...`);
    console.log(`Vite Environment: Loading modules for ${SITE_URL}...`);

    const vite = await createServer({
        server: { middlewareMode: true },
        appType: 'custom'
    });

    try {
        const { components } = await vite.ssrLoadModule('./src/data/components.ts');
        const { docTopicIds } = await vite.ssrLoadModule('./src/data/docs-ids.ts');

        let indexContent = `# RazorConsole\n\n> High-performance Blazor TUI framework, built on top of [Spectre.Console](https://spectreconsole.net).\n\n\n`;
        let fullContent = indexContent;

        indexContent += `## Documentation Guides\n\n`;
        for (const doc of docTopicIds) {
            const route = `/docs/${doc.id}`;
            indexContent += `- [${doc.title}](${SITE_URL}${route})\n`;

            const relativePath = doc.filePath.replace(/^website\//, '');
            const absolutePath = path.resolve(config.root, relativePath);

            if (fs.existsSync(absolutePath)) {
                const text = fs.readFileSync(absolutePath, 'utf8');
                fullContent += `\n---\n\n# Document: ${doc.title}\n\n${text}\n`;
            }
        }

        indexContent += `\n## Components API\n\n`;
        for (const comp of components) {
            const route = `/components/${comp.name.toLowerCase()}`;
            indexContent += `- [${comp.name}](${SITE_URL}${route}): ${comp.description}\n`;
            fullContent += `\n---\n\n# Component: ${comp.name}\n\n${comp.description}\n\n`;
        }

        if (fs.existsSync(DIST_DIR)) {
            fs.writeFileSync(path.join(DIST_DIR, 'llms.txt'), indexContent);
            fs.writeFileSync(path.join(DIST_DIR, 'llms-full.txt'), fullContent);
        }

        console.log('✅ llms.txt generated successfully via Vite SSR Bridge.');

    } catch (e) {
        console.error('❌ Failed to generate documentation:', e);
    } finally {
        await vite.close();
    }
}

generate();