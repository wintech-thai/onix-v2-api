#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
uname = 'username1'

### GetUserById
apiUrl = "api/OrganizationUser/org/#{orgId}/action/GetUserAllowedMenu/#{uname}"
result = make_request(:get, apiUrl, nil)
puts(result)
