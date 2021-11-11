<script>
    import { onMount } from "svelte";

    let settings;

    onMount(async () => {
        settings = await (await fetch("/api/settings")).json();
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
</script>

<h1>ABP.io Generator</h1>

{#if settings}
<div>
    <h2>Settings</h2>
    <div>
        <label for="projectPathSetting">Project path</label>
        <input name="projectPathSetting" type="text" bind:value={settings.projectPath} on:blur={updateSettings} />
    </div>
</div>
{/if}
