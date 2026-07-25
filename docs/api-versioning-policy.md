# API Versioning Policy

## Purpose

This API uses versioning to allow new features and improvements without breaking existing clients. Multiple API versions may be supported at the same time to provide a smooth migration path.

## Breaking Changes

The following changes are considered breaking changes and require a new API version:

- Removing a response field
- Renaming a response field
- Changing the meaning or data type of a field
- Changing HTTP status codes
- Tightening validation rules that reject previously valid requests
- Changing the default sorting behavior of an endpoint

## Non-Breaking Changes

The following changes are considered additive and do not require a new version:

- Adding a new optional response field
- Adding a new endpoint
- Adding a new optional query parameter
- Adding new functionality without changing existing behavior

## Deprecation and Sunset

When a new API version is released, the previous version will remain available for at least six months. During this period, deprecated versions will include:

- Deprecation header
- Sunset header
- Link header pointing to the successor version

This gives client applications enough time to migrate without service disruption.

## Communication

API version changes are communicated through:

- HTTP Deprecation, Sunset, and Link headers
- CHANGELOG updates
- Email notifications to API consumers
- Scheduled reminders before the sunset date

## Version Migration

Clients may migrate directly from one version to any newer supported version. They are not required to upgrade through every intermediate version.