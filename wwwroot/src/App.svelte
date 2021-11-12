<script>
    import { onMount } from "svelte";
    import * as signalR from "@microsoft/signalr";

    let realtimeConnection;
    let connected = true;
    let settings;

    onMount(async () => {
        settings = await (await fetch("/api/settings")).json();

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

    async function openTemplatesFolder() {
        await fetch("/api/actions/open-templates-folder");
    }

    async function updateSettings() {
        await fetch("/api/settings", {
            method: "PUT",
            body: JSON.stringify(settings),
            headers: {
                "Content-Type": "application/json"
            }
        });
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
    </div>
    {/if}
</div>