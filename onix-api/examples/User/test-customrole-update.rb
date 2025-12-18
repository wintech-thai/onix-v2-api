#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
id = '78ed5b6f-c8ec-4679-b694-50f10abab1ab'

apiUrl = "api/CustomRole/org/#{orgId}/action/UpdateCustomRoleById/#{id}"

param = {
  RoleName: "OWNER",
  RoleDescription: "Able to do anything XXX",
  Tags: "testing",
  Permissions: [
    {
      ControllerName: "AccountDoc",
      ApiPermissions: [
        { ApiName: "GetAccountDocCashInvoices", ControllerName: "AccountDoc", IsAllowed: true },
        { ApiName: "AddAccountDocCashInvoice",  ControllerName: "AccountDoc", IsAllowed: true },
      ]
    }
  ]
}

result = make_request(:post, apiUrl, param)

json = result.to_json
puts(json)
