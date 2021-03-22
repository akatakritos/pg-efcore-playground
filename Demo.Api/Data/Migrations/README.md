# Migrations

**IMPORTANT**: Make sure you set the Build Action on the script to Embedded Resource.

Filename format: `{Type}{Date}{Sequence}__{Name}.sql`
* `Type`: `V` or `R`
    * `V` - one time scripts
    * `R` - rerunnable scripts. Should be idempotent, will run every migration
* `Date` in `YYYY.MM.DD` format
* `Sequence` - sequence number for multiple scripts in a single day

Files are executed in sort order.

Do not modify a V script once it has been run, it will have no effect.
