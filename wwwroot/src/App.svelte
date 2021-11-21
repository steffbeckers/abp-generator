<script>
    import { onMount } from "svelte";
    import * as signalR from "@microsoft/signalr";
    
    let version;
    let settings;

    let realtimeConnection;
    let connected = true;

    let snippetTemplates = [];
    let selectedSnippetTemplateFullPath;
    let snippetTemplate;

    onMount(async () => {
        fetch("/api/version")
            .then((response) => response.text())
            .then((data) => {
                version = data;
            });

        fetch("/api/settings")
            .then((response) => response.json())
            .then((data) => {
                settings = data;
            });

        fetch("/api/templates/snippets")
            .then((response) => response.json())
            .then((data) => {
                snippetTemplates = data;
                selectedSnippetTemplateFullPath = snippetTemplates[0].fullPath
                setSnippetTemplate();
            });

        realtimeConnection = new signalR.HubConnectionBuilder()
            .withUrl("/signalr-hubs/realtime")
            .withAutomaticReconnect()
            .build();
        
        realtimeConnection.on("SettingsUpdated", (updatedSettings) => {
            settings = updatedSettings;
        });

        realtimeConnection.on("SnippetTemplateCreated", (createdSnippetTemplate) => {
            snippetTemplates.unshift(createdSnippetTemplate);

            selectedSnippetTemplateFullPath = createdSnippetTemplate.fullPath;
            setSnippetTemplate();
        });

        realtimeConnection.on("SnippetTemplateUpdated", (updatedSnippetTemplate) => {
            let updatedSnippetTemplateIndex = snippetTemplates.map(x => x.fullPath).indexOf(updatedSnippetTemplate.fullPath);

            if (updatedSnippetTemplateIndex > -1) {
                snippetTemplates[updatedSnippetTemplateIndex] = updatedSnippetTemplate;

                selectedSnippetTemplateFullPath = updatedSnippetTemplate.fullPath;
                setSnippetTemplate();
            }
        });

        realtimeConnection.on("SnippetTemplateDeleted", (deletedSnippetTemplateFullPath) => {
            let deletedSnippetTemplateIndex = snippetTemplates.map(x => x.fullPath).indexOf(deletedSnippetTemplateFullPath);

            if (deletedSnippetTemplateIndex > -1) {
                if (deletedSnippetTemplateFullPath == selectedSnippetTemplateFullPath && snippetTemplates.length > 0) {
                    selectedSnippetTemplateFullPath = snippetTemplates[0].fullPath;
                    setSnippetTemplate();
                }

                snippetTemplates.splice(deletedSnippetTemplateIndex, 1);
            }
        });

        realtimeConnection.on("SnippetTemplatesReloaded", (reloadedSnippetTemplates) => {
            snippetTemplates = reloadedSnippetTemplates;
            setSnippetTemplate();
        });

        realtimeConnection.onclose(() => {
            connected = false;
        });

        realtimeConnection.onreconnecting(() => {
            connected = false;
        })

        realtimeConnection.onreconnected(() => {
            connected = true;
        })

        await realtimeConnection.start();
    })

    async function updateSettings() {
        await fetch("/api/settings", {
            method: "PUT",
            body: JSON.stringify(settings),
            headers: {
                "Content-Type": "application/json"
            }
        });
    }

    async function openSettingsJson() {
        await fetch("/api/settings/open-json");
    }

    async function openTemplatesFolder() {
        await fetch("/api/templates/snippets/open-folder");
    }

    function setSnippetTemplate() {
        let selectedSnippetTemplateIndex = snippetTemplates.map(x => x.fullPath).indexOf(selectedSnippetTemplateFullPath);
        if (selectedSnippetTemplateIndex > -1) {
            snippetTemplate = snippetTemplates[selectedSnippetTemplateIndex];
        }
    }
</script>

<div class="container">
    <h1>ABP.io Generator</h1>
    {#if version}<div>Version: {version}</div>{/if}
    {#if !connected}
    <h2 style="color: red; font-weigth: bold">
        CLI connection lost!
    </h2>
    {:else}
    <div>
        <h2>Settings</h2>
        <div>
            <button on:click={openSettingsJson} type="button">Open JSON</button>
        </div>
        {#if settings}
        <!-- <div style="white-space: pre">
            {JSON.stringify(settings, null, 2)}
        </div> -->
        <div style="display: flex; flex-direction: column">
            <div style="flex: 1 1">
                <label for="projectPathSetting">Project path</label>
                <input bind:value={settings.projectPath} on:blur={updateSettings} type="text" id="projectPathSetting" />
            </div>
            <div style="display: flex; gap: 12px">
                <div style="flex: 1 1">
                    <label for="projectNameSetting">Project.Name</label>
                    <input bind:value={settings.context.project.name} on:blur={updateSettings} type="text" id="projectNameSetting" />
                </div>
                <div style="flex: 1 1">
                    <label for="companyNameSetting">Project.CompanyName</label>
                    <input bind:value={settings.context.project.companyName} type="text" id="companyNameSetting" disabled />
                </div>
                <div style="flex: 1 1">
                    <label for="productNameSetting">Project.ProductName</label>
                    <input bind:value={settings.context.project.productName} type="text" id="productNameSetting" disabled />
                </div>
            </div>
            <div style="display: flex; gap: 12px">
                <div style="flex: 1 1">
                    <label for="aggregateRootNameSetting">AggregateRoot.Name</label>
                    <input bind:value={settings.context.aggregateRoot.name} on:blur={updateSettings} type="text" id="aggregateRootNameSetting" />
                </div>
                <div style="flex: 1 1">
                    <label for="aggregateRootNamePluralSetting">AggregateRoot.NamePlural</label>
                    <input bind:value={settings.context.aggregateRoot.namePlural} on:blur={updateSettings} type="text" id="aggregateRootNamePluralSetting" />
                </div>
            </div>
        </div>
        {/if}
    </div>
    <div>
        <h2>Templates</h2>
        <div>
            <button on:click={openTemplatesFolder} type="button">Open folder</button>
        </div>
        {#if snippetTemplates}
        <div style="display: flex; gap: 12px">
            <select bind:value={selectedSnippetTemplateFullPath} on:change={setSnippetTemplate} style="flex: 1 1">
                {#each snippetTemplates as snippetTemplate}
                <option value={snippetTemplate.fullPath}>{snippetTemplate.outputPath}</option>
                {/each}
            </select>
            <!-- <button type="button" style="flex: 0 1">Generate</button> -->
        </div>
        {/if}
        {#if snippetTemplate}
        <div style="white-space: pre">
            {snippetTemplate.output}
        </div>
        {/if}
    </div>
    {/if}
</div>