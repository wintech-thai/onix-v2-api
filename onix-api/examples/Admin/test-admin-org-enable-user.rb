#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = "global"
keyFile = ".token"
userOrgId = "ppm-alfa888"
orgUserId = '37f90e58-428b-45aa-82cd-2b966e06c89e'

### 
apiUrl = "admin-api/AdminOrganization/org/#{orgId}/action/EnableOrgUserById/#{userOrgId}/#{orgUserId}"
#apiUrl = "admin-api/AdminOrganization/org/#{orgId}/action/DisableOrgUserById/#{userOrgId}/#{orgUserId}"

param = nil

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result)
