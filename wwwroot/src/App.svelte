<script>
    import { onMount } from "svelte";
    import * as signalR from "@microsoft/signalr";
    
    let settings;

    let realtimeConnection;
    let connected = true;

    let snippetTemplates;
    let selectedSnippetTemplateIndex = 0;
    let snippetTemplate;

    onMount(async () => {
        fetch("/api/settings")
            .then((response) => response.json())
            .then((data) => {
                settings = data;
            });

        fetch("/api/templates/snippets")
            .then((response) => response.json())
            .then((data) => {
                snippetTemplates = data;
                updateSnippetTemplate();
            });

        realtimeConnection = new signalR.HubConnectionBuilder()
            .withUrl("/signalr-hubs/realtime")
            .withAutomaticReconnect()
            .build();
        
        realtimeConnection.on("SettingsUpdated", (updatedSettings) => {
            settings = updatedSettings;
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

    async function openTemplatesFolder() {
        await fetch("/api/templates/open-folder");
    }

    function updateSnippetTemplate() {
        snippetTemplate = snippetTemplates[selectedSnippetTemplateIndex];
    }
</script>

<div class="container">
    <h1>ABP.io Generator</h1>
    {#if !connected}
    <h2 style="color: red; font-weigth: bold">
        CLI connection lost!
    </h2>
    {:else}
    <div>
        <h2>Settings</h2>
        {#if settings}
        <div>
            <label for="projectPathSetting">Project path</label>
            <input style="width: 100%;" name="projectPathSetting" type="text" bind:value={settings.projectPath} on:blur={updateSettings} />
        </div>
        {/if}
    </div>
    <div>
        <h2>Templates</h2>
        <div>
            <button type="button" on:click={openTemplatesFolder}>Open folder</button>
        </div>
        <h3>Snippets</h3>
        {#if snippetTemplates}
        <div>
            <select bind:value={selectedSnippetTemplateIndex} on:change={updateSnippetTemplate}>
                {#each snippetTemplates as snippetTemplate, index}
                <option value={index}>{snippetTemplate.outputPath}</option>
                {/each}
            </select>
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