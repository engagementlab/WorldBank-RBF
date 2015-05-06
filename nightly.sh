#/bin/sh
 
# World Bank RBF
# Created by Engagement Lab, 2015
# ==============
#  nightly.sh
#  Nightly build actions; called from separate Jenkins job.

#  Created by Johnny Richardson on 5/6/15.
# ==============

OUTPUT_NAME=$1;
EXTERNAL_BUILDS_DIR="/Library/BuildArtifacts";
GOOGLE_DRIVE_DIR="$HOME/Google\ Drive";

# Get file info for the current _Nightly tar via gdrive (https://github.com/prasmussen/gdrive)
current_file_info="$(drive list -t $OUTPUT_NAME""_Nightly)";

# Convert info to array (string is space-delimited)
arr_file_info=($current_file_info);

# We need the ID, which is the fifth index 
current_file_id=${arr_file_info[4]};

# Check if file is on drive
if [ -z "$current_file_id" ]; then
	echo "File not found on drive"
else
	echo $current_file_id;
	
	# Delete the current _Nightly tar from Drive
	drive delete -i $current_file_id;
fi

# Set target path for local tar file
target_tar="$EXTERNAL_BUILDS_DIR/$OUTPUT_NAME.tgz";

# Upload latest build tar to Drive
drive upload -f $target_tar -t "$OUTPUT_NAME""_Nightly.tgz";

# Now get new file ID and share the file with all users
new_file_info="$(drive list -t $OUTPUT_NAME""_Nightly)";

arr_new_file_info=($new_file_info);

new_file_id=${arr_new_file_info[4]};

# Share file
drive share -i $new_file_id;

# Get URL
url="$(drive url -i $new_file_id)";

# Tell slack about the new file
echo "$OUTPUT_NAME Nightly build for $(date +"%D") posted ($url)"