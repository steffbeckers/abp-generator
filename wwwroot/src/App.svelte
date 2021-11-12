<script>
    import { onMount } from "svelte";

    let settings;

    onMount(async () => {
        settings = await (await fetch("/api/settings")).json();
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
</div>