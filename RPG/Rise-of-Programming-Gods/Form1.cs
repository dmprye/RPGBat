using Rise_of_Programming_Gods.Characters;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing.Imaging;

namespace Rise_of_Programming_Gods
{
    public partial class Form1 : Form
    {
        private readonly BattleManager battleManager;
        private Timer battleTimer;
        private Timer animationTimer;
        private ToolTip toolTip;

        // Animation variables
        private string player1CurrentAnimation = "Idle"; // Track current animation state for Player 1
        private string player2CurrentAnimation = "Idle"; // Track current animation state for Player 2
        private int animatingPlayer = 0; // 0: none, 1: player1, 2: player2
        private Image currentAnimatingSpriteSheet;
        private int currentAnimatingFrameIndex = 0;
        private int currentAnimatingTotalFrames = 0;
        private int currentAnimatingFrameWidth = 0;
        private int currentAnimatingFrameHeight = 0;
        private int player1AnimationFrameIndex = 0; // Current frame index for Player 1's animation
        private int player2AnimationFrameIndex = 0; // Current frame index for Player 2's animation

        // Animation Request Queue
        private Queue<(int playerNumber, string animationState)> animationQueue;

        public Form1()
        {
            InitializeComponent();
            battleManager = new BattleManager();
            toolTip = new ToolTip();
            InitializeBattleTimer();
            InitializeAnimationTimer();
            SetupForm();
            // Initialize the animation queue
            animationQueue = new Queue<(int playerNumber, string animationState)>();
        }

        private int player1Health = 100;
        private int player2Health = 100;




        private void InitializeBattleTimer()
        {
            battleTimer = new Timer();
            battleTimer.Interval = 1000; // 1 second between turns
            battleTimer.Tick += BattleTimer_Tick;
        }

        private void InitializeAnimationTimer()
        {
            animationTimer = new Timer();
            animationTimer.Interval = 100; // Adjust for desired animation speed (e.g., 100ms)
            animationTimer.Tick += AnimationTimer_Tick;
        }

        private void SetupForm()
        {
            // Set the form background image and stretch it to fill the form
            this.BackgroundImage = Properties.Resources.classroombg;
            this.BackgroundImageLayout = ImageLayout.Stretch;

            // Set PictureBox SizeMode to Normal for custom drawing
            pictureBox1.SizeMode = PictureBoxSizeMode.Normal;

            // Set the background color of pictureBox1 to be semi-transparent
            // This allows the form's background image to show through
            pictureBox1.BackColor = Color.FromArgb(128, Color.Black); // 128 alpha (semi-transparent), Black color

            // Existing setup code ...
            string[] characterTypes = new string[]
            {
        "PauCoder",
        "RogerRipper",
        "StarLord",
        "RyenVizier"
            };

            cmbPlayer1Type.Items.AddRange(characterTypes);
            cmbPlayer2Type.Items.AddRange(characterTypes);
            cmbPlayer1Type.SelectedIndex = 0;
            cmbPlayer2Type.SelectedIndex = 0;

            // Attach event handlers for character type changes
            cmbPlayer1Type.SelectedIndexChanged += cmbPlayer1Type_SelectedIndexChanged;
            cmbPlayer2Type.SelectedIndexChanged += cmbPlayer2Type_SelectedIndexChanged;

            txtBattleLog.Multiline = true;
            txtBattleLog.ScrollBars = RichTextBoxScrollBars.Vertical;
            txtBattleLog.ReadOnly = true;

            lblPlayer1Health.Text = "Health: 100";
            lblPlayer2Health.Text = "Health: 100";

            // Remove background from controls to see the form background if needed
        }

        private void cmbPlayer1Type_SelectedIndexChanged(object sender, EventArgs e)
        {
            // When player 1 character changes, update the display to show idle pose
            // Ensure battle is not ongoing before manually updating display based on selection
            if (!battleTimer.Enabled)
            {
                player1CurrentAnimation = "Idle"; // Set state to Idle
                player1AnimationFrameIndex = 0; // Reset frame index
                // No need to set animatingPlayer here, DrawCharactersBasedOnState handles it
                DrawCharactersBasedOnState(); // Update the display
            }
        }

        private void cmbPlayer2Type_SelectedIndexChanged(object sender, EventArgs e)
        {
            // When player 2 character changes, update the display to show idle pose
            // Ensure battle is not ongoing before manually updating display based on selection
            if (!battleTimer.Enabled)
            {
                player2CurrentAnimation = "Idle"; // Set state to Idle
                player2AnimationFrameIndex = 0; // Reset frame index
                // No need to set animatingPlayer here, DrawCharactersBasedOnState handles it
                DrawCharactersBasedOnState(); // Update the display
            }
        }

        private void btnStartBattle_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate player names
                if (string.IsNullOrWhiteSpace(txtPlayer1Name.Text) || string.IsNullOrWhiteSpace(txtPlayer2Name.Text))
                {
                    MessageBox.Show("Please enter names for both players.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Validate player types are selected
                if (cmbPlayer1Type.SelectedItem == null || cmbPlayer2Type.SelectedItem == null)
                {
                    MessageBox.Show("Please select types for both players.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Reset health values and update UI
                ResetHealthUI();

                // Clear battle log before starting
                txtBattleLog.Clear();

                // Create player characters
                CodingWarrior player1 = CreateCharacter(txtPlayer1Name.Text, cmbPlayer1Type.SelectedItem.ToString());
                CodingWarrior player2 = CreateCharacter(txtPlayer2Name.Text, cmbPlayer2Type.SelectedItem.ToString());

                // Initialize the battle
                battleManager.InitializeBattle(player1, player2);

                // Display initial battle log
                UpdateBattleLog();

                // Set initial animation states to Idle and start animation timer
                player1CurrentAnimation = "Idle";
                player2CurrentAnimation = "Idle";
                animatingPlayer = 0; // No action animation initially
                if (!animationTimer.Enabled)
                {
                    animationTimer.Start();
                }

                // Start the battle timer
                battleTimer.Start();

                // Disable the start button while battle is ongoing
                btnStartBattle.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting battle: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResetHealthUI()
        {
            player1Health = 100;
            player2Health = 100;

            lblPlayer1Health.Text = "Health: 100";
            progressBarPlayer1.Value = 100;

            lblPlayer2Health.Text = "Health: 100";
            progressBarPlayer2.Value = 100;
        }




        private CodingWarrior CreateCharacter(string name, string type)
        {
            switch (type)
            {
                case "PauCoder":
                    return new PauCoder(name);
                case "RogerRipper":
                    return new RogerRipper(name);
                case "StarLord":
                    return new StarLord(name);
                case "RyenVizier":
                    return new RyenVizier(name);
                default:
                    throw new ArgumentException($"Unknown character type: {type}");
            }
        }

        private void BattleTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // Execute one turn of the battle; returns false if battle is over
                if (!battleManager.ExecuteTurn())
                {
                    battleTimer.Stop();
                    // animationTimer.Stop(); // Stop animation timer when battle ends (Moved below)
                    btnStartBattle.Enabled = true;

                    // Check which player lost and set their animation to dead
                    if (battleManager.Player1.Health <= 0)
                    {
                        player1CurrentAnimation = $"{battleManager.Player1.GetType().Name}_Dead";
                        player1AnimationFrameIndex = 0;
                        // Set animatingPlayer to the player who died to trigger animation update
                        animatingPlayer = 1;
                    }
                    if (battleManager.Player2.Health <= 0)
                    {
                        player2CurrentAnimation = $"{battleManager.Player2.GetType().Name}_Dead";
                        player2AnimationFrameIndex = 0;
                        // Set animatingPlayer to the player who died to trigger animation update
                        animatingPlayer = 2;
                    }

                    // Clear animation queue 
                    animationQueue.Clear();
                    
                    // Force one final redraw to show the initial frame of the death animation
                    DrawCharactersBasedOnState();

                    // We don't stop the animation timer immediately here. 
                    // The AnimationTimer_Tick will handle the death animation until its last frame.
                    // It will automatically transition to Idle or stop if needed at the end of the death animation.
                }

                // Update the battle log text area
                UpdateBattleLog();

                // Update health UI after the turn
                UpdateHealthDisplays();

                // Update the fighting scene in pictureBox1
                UpdateFightingScene();

            }
            catch (Exception ex)
            {
                battleTimer.Stop();
                animationTimer.Stop(); // Also stop animation timer on error
                MessageBox.Show($"Error during battle: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnStartBattle.Enabled = true;
            }
        }

        private void UpdateFightingScene()
        {
            // Simple animation: just re-display the combined image each tick
            // In a more advanced scenario, you would cycle through attack/defense/damage animation frames here.
            if (battleManager.Player1 != null && battleManager.Player2 != null)
            {
                string player1Type = battleManager.Player1.GetType().Name;
                string player2Type = battleManager.Player2.GetType().Name;

                // Determine the desired animation state based on the last log entry
                string player1Name = battleManager.Player1.Name;
                string player2Name = battleManager.Player2.Name;
                string lastLogEntry = battleManager.BattleLog.Count > 0 ? battleManager.BattleLog[battleManager.BattleLog.Count - 1] : "";

                Debug.WriteLine($"UpdateFightingScene: Player1: {player1Name} ({player1Type}), Player2: {player2Name} ({player2Type})");
                Debug.WriteLine($"UpdateFightingScene: Last Log Entry: {lastLogEntry}");

                // Determine the intended state for this turn based on the log
                string intendedPlayer1State = "Idle";
                string intendedPlayer2State = "Idle";

                Debug.WriteLine($"UpdateFightingScene: Determining intended states based on log entry: '{lastLogEntry}'");

                // Check for Player 1's actions
                Debug.WriteLine($"UpdateFightingScene: Checking Player 1 ({player1Name}) actions...");
                Debug.WriteLine($"UpdateFightingScene: lastLogEntry contains player1Name: {lastLogEntry.Contains(player1Name)}");
                if (lastLogEntry.Contains(player1Name))
                {
                    Debug.WriteLine("Player 1 name found in log entry.");
                    // Check for PauCoder's attack
                    if (player1Type == "PauCoder" && lastLogEntry.Contains("strikes!"))
                    {
                        Debug.WriteLine("Player 1 is PauCoder and log contains 'strikes!'. Intending state to PauCoder_Attack_1.");
                        intendedPlayer1State = "PauCoder_Attack_1";
                    }
                    // Check for RyenVizier's attack
                    else if (player1Type == "RyenVizier" && lastLogEntry.Contains("casts!"))
                    {
                        Debug.WriteLine("Player 1 is RyenVizier and log contains 'casts!'. Intending state to RyenVizier_Attack_1.");
                        intendedPlayer1State = "RyenVizier_Attack_1";
                    }
                    // Check for RogerRipper's attack
                    else if (player1Type == "RogerRipper" && lastLogEntry.Contains("slashes!"))
                    {
                        Debug.WriteLine("Player 1 is RogerRipper and log contains 'slashes!'. Intending state to RogerRipper_Attack_1.");
                        intendedPlayer1State = "RogerRipper_Attack_1";
                    }
                    // Check for StarLord's attack
                    else if (player1Type == "StarLord" && lastLogEntry.Contains("attacks!"))
                    {
                        Debug.WriteLine("Player 1 is StarLord and log contains 'attacks!'. Intending state to StarLord_Attack_1.");
                        intendedPlayer1State = "StarLord_Attack_1";
                    }
                }

                // Check for Player 2's actions
                Debug.WriteLine($"UpdateFightingScene: Checking Player 2 ({player2Name}) actions...");
                Debug.WriteLine($"UpdateFightingScene: lastLogEntry contains player2Name: {lastLogEntry.Contains(player2Name)}");
                if (lastLogEntry.Contains(player2Name))
                {
                    Debug.WriteLine("Player 2 name found in log entry.");
                    // Check for PauCoder's attack
                    if (player2Type == "PauCoder" && lastLogEntry.Contains("strikes!"))
                    {
                        Debug.WriteLine("Player 2 is PauCoder and log contains 'strikes!'. Intending state to PauCoder_Attack_1.");
                        intendedPlayer2State = "PauCoder_Attack_1";
                    }
                    // Check for RyenVizier's attack
                    else if (player2Type == "RyenVizier" && lastLogEntry.Contains("casts!"))
                    {
                        Debug.WriteLine("Player 2 is RyenVizier and log contains 'casts!'. Intending state to RyenVizier_Attack_1.");
                        intendedPlayer2State = "RyenVizier_Attack_1";
                    }
                    // Check for RogerRipper's attack
                    else if (player2Type == "RogerRipper" && lastLogEntry.Contains("slashes!"))
                    {
                        Debug.WriteLine("Player 2 is RogerRipper and log contains 'slashes!'. Intending state to RogerRipper_Attack_1.");
                        intendedPlayer2State = "RogerRipper_Attack_1";
                    }
                    // Check for StarLord's attack
                    else if (player2Type == "StarLord" && lastLogEntry.Contains("attacks!"))
                    {
                        Debug.WriteLine("Player 2 is StarLord and log contains 'attacks!'. Intending state to StarLord_Attack_1.");
                        intendedPlayer2State = "StarLord_Attack_1";
                    }
                }

                // Now, compare intended states with current states and trigger animations if needed
                // This ensures animations are started only when the state *changes* to an action state

                Debug.WriteLine($"UpdateFightingScene: Intended Player 1 State: {intendedPlayer1State}, Current Player 1 State: {player1CurrentAnimation}");
                Debug.WriteLine($"UpdateFightingScene: Intended Player 2 State: {intendedPlayer2State}, Current Player 2 State: {player2CurrentAnimation}");

                // Enqueue animation requests based on intended states
                if (intendedPlayer1State != "Idle")
                {
                    // Add Player 1's action animation to the queue
                    animationQueue.Enqueue((1, intendedPlayer1State));
                    Debug.WriteLine($"UpdateFightingScene: Enqueued Player 1 animation: {intendedPlayer1State}");
                }

                if (intendedPlayer2State != "Idle")
                {
                    // Add Player 2's action animation to the queue
                    animationQueue.Enqueue((2, intendedPlayer2State));
                    Debug.WriteLine($"UpdateFightingScene: Enqueued Player 2 animation: {intendedPlayer2State}");
                }

                // Removed direct animation triggering logic from here.
                // Animations will be dequeued and started by the AnimationTimer_Tick.
            }
        }

        private void DrawCharactersBasedOnState()
        {
            if (battleManager.Player1 == null || battleManager.Player2 == null) return; // Ensure players exist

            string player1Type = battleManager.Player1.GetType().Name;
            string player2Type = battleManager.Player2.GetType().Name;

            Image player1ImageToDraw = null;
            Image player2ImageToDraw = null;

            Debug.WriteLine($"DrawCharactersBasedOnState: Player1 State: {player1CurrentAnimation}, Frame: {player1AnimationFrameIndex}");
            Debug.WriteLine($"DrawCharactersBasedOnState: Player2 State: {player2CurrentAnimation}, Frame: {player2AnimationFrameIndex}");

            // Get the idle sprites for reference positioning
            Image player1IdleImage = GetCharacterImage(player1Type);
            Image player2IdleImage = GetCharacterImage(player2Type);

            // Determine and get the image for Player 1 based on their animation state and frame index
            if (player1CurrentAnimation == "Idle")
            {
                player1ImageToDraw = player1IdleImage;
            }
            else
            {
                Image spriteSheet;
                int frameWidth = 0;
                int frameHeight = 0;
                (spriteSheet, frameWidth, frameHeight) = GetAnimationDetails(player1CurrentAnimation);
                if (spriteSheet != null && frameWidth > 0 && frameHeight > 0)
                {
                    player1ImageToDraw = GetSpriteFrame(spriteSheet, player1AnimationFrameIndex, frameWidth, frameHeight);
                }
            }

            // Determine and get the image for Player 2 based on their animation state and frame index
            if (player2CurrentAnimation == "Idle")
            {
                player2ImageToDraw = player2IdleImage;
            }
            else
            {
                Image spriteSheet;
                int frameWidth = 0;
                int frameHeight = 0;
                (spriteSheet, frameWidth, frameHeight) = GetAnimationDetails(player2CurrentAnimation);
                if (spriteSheet != null && frameWidth > 0 && frameHeight > 0)
                {
                    player2ImageToDraw = GetSpriteFrame(spriteSheet, player2AnimationFrameIndex, frameWidth, frameHeight);
                }
            }

            // Create a new bitmap to draw on
            Bitmap combinedImage = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            using (Graphics g = Graphics.FromImage(combinedImage))
            {
                g.Clear(Color.Transparent);

                // Define a fixed padding from the bottom of the picture box
                int bottomPadding = 30; // Adjusted padding
                int groundLineY = pictureBox1.Height - bottomPadding;

                // Calculate the available vertical space
                int availableHeightForScaling = pictureBox1.Height - bottomPadding;

                // Define spacing between characters
                int spacing = 50;

                // Calculate scale factors primarily based on idle sprite dimensions to fit within available space
                float scale1 = 5.0f;
                float scale2 = 5.0f;

                if (player1IdleImage != null && availableHeightForScaling > 0)
                {
                    // Calculate scale based on fitting the idle height into available vertical space
                    float heightScale1 = (float)availableHeightForScaling / player1IdleImage.Height;
                    // Calculate scale based on fitting two idle sprites plus spacing into total width
                    // Use the defined spacing
                    float widthScale1 = (float)(pictureBox1.Width - spacing) / (player1IdleImage.Width * 2); // Consider spacing and two characters
                    
                    scale1 = Math.Min(heightScale1, widthScale1); // Use the minimum to ensure it fits both ways based on idle size
                }

                if (player2IdleImage != null && availableHeightForScaling > 0)
                {
                    // Calculate scale based on fitting the idle height into available vertical space
                    float heightScale2 = (float)availableHeightForScaling / player2IdleImage.Height;
                    // Calculate scale based on fitting two idle sprites plus spacing into total width
                    // Use the defined spacing
                     float widthScale2 = (float)(pictureBox1.Width - spacing) / (player2IdleImage.Width * 2); // Consider spacing and two characters

                    scale2 = Math.Min(heightScale2, widthScale2); // Use the minimum to ensure it fits both ways based on idle size
                }

                // Calculate scaled dimensions for idle sprites (for positioning reference) using the determined scale
                int scaledWidth1_Idle = (int)((player1IdleImage != null ? player1IdleImage.Width : 0) * scale1);
                int scaledHeight1_Idle = (int)((player1IdleImage != null ? player1IdleImage.Height : 0) * scale1);
                int scaledWidth2_Idle = (int)((player2IdleImage != null ? player2IdleImage.Height : 0) * scale2);
                int scaledHeight2_Idle = (int)((player2IdleImage != null ? player2IdleImage.Height : 0) * scale2);

                // Calculate scaled dimensions for current animation frames using the determined scale
                int scaledWidth1 = player1ImageToDraw != null ? (int)(player1ImageToDraw.Width * scale1) : 0;
                int scaledHeight1 = player1ImageToDraw != null ? (int)(player1ImageToDraw.Height * scale1) : 0;
                int scaledWidth2 = player2ImageToDraw != null ? (int)(player2ImageToDraw.Width * scale2) : 0;
                int scaledHeight2 = player2ImageToDraw != null ? (int)(player2ImageToDraw.Height * scale2) : 0;

                // Calculate spacing and total width based on idle widths for consistent positioning
                int totalCharactersWidth = scaledWidth1_Idle + spacing + scaledWidth2_Idle;
                int startX = (pictureBox1.Width - totalCharactersWidth) / 2;

                // Draw Player 1
                if (player1ImageToDraw != null)
                {
                    // Calculate horizontal position to center the current frame within the idle sprite's space
                    int x1 = startX + (scaledWidth1_Idle - scaledWidth1) / 2;

                    // Calculate vertical position to align the bottom of the current scaled frame with the ground line.
                    int y1 = groundLineY - scaledHeight1;

                    // Apply character-specific vertical offsets for specific animations (experimental)
                    int characterSpecificVerticalOffset1 = 0;
                    switch (player1CurrentAnimation)
                    {
                        case "PauCoder_Attack_1":
                             // This value might need tuning to align PauCoder's Attack_1
                            characterSpecificVerticalOffset1 = -20; // Example offset: move sprite up
                            break;
                        case "RogerRipper_Attack_1":
                             // This value might need tuning to align RogerRipper's Attack_1
                            characterSpecificVerticalOffset1 = -20; // Example offset: move sprite up
                            break;
                        case "StarLord_Attack_1":
                             // This value might need tuning to align StarLord's Attack_1
                            characterSpecificVerticalOffset1 = -20; // Example offset: move sprite up
                            break;
                        case "RyenVizier_Attack_1":
                             // This value might need tuning to align RyenVizier's Attack_1
                            characterSpecificVerticalOffset1 = -20; // Example offset: move sprite up
                            break;
                        // Add cases for other animations that need specific offsets
                    }
                    y1 += characterSpecificVerticalOffset1;

                    g.DrawImage(player1ImageToDraw, new Rectangle(x1, y1, scaledWidth1, scaledHeight1));
                }

                // Draw Player 2
                if (player2ImageToDraw != null)
                {
                    // Calculate horizontal position to center the current frame within the idle sprite's space
                    int x2 = startX + scaledWidth1_Idle + spacing + (scaledWidth2_Idle - scaledWidth2) / 2;

                    // Calculate vertical position to align the bottom of the current scaled frame with the ground line.
                    int y2 = groundLineY - scaledHeight2;

                     // Apply character-specific vertical offsets for specific animations (experimental)
                    int characterSpecificVerticalOffset2 = 0;
                    switch (player2CurrentAnimation)
                    {
                        case "PauCoder_Attack_1":
                             // This value might need tuning to align PauCoder's Attack_1
                            characterSpecificVerticalOffset2 = -20; // Example offset: move sprite up
                            break;
                        case "RogerRipper_Attack_1":
                             // This value might need tuning to align RogerRipper's Attack_1
                            characterSpecificVerticalOffset2 = -20; // Example offset: move sprite up
                            break;
                        case "StarLord_Attack_1":
                             // This value might need tuning to align StarLord's Attack_1
                            characterSpecificVerticalOffset2 = -20; // Example offset: move sprite up
                            break;
                        case "RyenVizier_Attack_1":
                             // This value might need tuning to align RyenVizier's Attack_1
                            characterSpecificVerticalOffset2 = -20; // Example offset: move sprite up
                            break;
                        // Add cases for other animations that need specific offsets
                    }
                    y2 += characterSpecificVerticalOffset2;

                    // Flip Player 2's image horizontally
                    player2ImageToDraw.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    g.DrawImage(player2ImageToDraw, new Rectangle(x2, y2, scaledWidth2, scaledHeight2));
                    player2ImageToDraw.RotateFlip(RotateFlipType.RotateNoneFlipX);
                }

                // Draw the combined image onto the picture box
                pictureBox1.Image = combinedImage;
            }

            Debug.WriteLine($"DrawCharactersBasedOnState: Player1 State: {player1CurrentAnimation}, Player2 State: {player2CurrentAnimation}");
            Debug.WriteLine($"DrawCharactersBasedOnState: Currently Animating Player: {animatingPlayer}");
        }

        private Image GetSpriteFrame(Image spriteSheet, int frameIndex, int frameWidth, int frameHeight)
        {
            Debug.WriteLine($"GetSpriteFrame received: SpriteSheet Size: {spriteSheet?.Size}, Frame Index: {frameIndex}, Frame Size: {frameWidth}x{frameHeight}");

            if (spriteSheet == null || frameIndex < 0 || frameWidth <= 0 || frameHeight <= 0) return null;

            int totalFrames = spriteSheet.Width / frameWidth; // Assuming frames are horizontal
            if (frameIndex >= totalFrames) return null;

            Rectangle sourceRect = new Rectangle(frameIndex * frameWidth, 0, frameWidth, frameHeight);
            Debug.WriteLine($"GetSpriteFrame: Calculated SourceRect: {sourceRect}");

            // Create a new bitmap to hold the frame
            Bitmap frameBitmap = new Bitmap(frameWidth, frameHeight);
            using (Graphics g = Graphics.FromImage(frameBitmap))
            {
                g.DrawImage(spriteSheet, 0, 0, sourceRect, GraphicsUnit.Pixel);
            }
            return frameBitmap;
        }

        private void UpdateHealthDisplays()
        {
            // Assuming battleManager has properties for Player1 and Player2:
            var player1 = battleManager.Player1;
            var player2 = battleManager.Player2;

            // Update labels to current health
            lblPlayer1Health.Text = $"Health: {Math.Max(0, player1.Health)}";
            lblPlayer2Health.Text = $"Health: {Math.Max(0, player2.Health)}";

            // Update progress bars, ensure value within range 0 - 100 (or max health)
            progressBarPlayer1.Value = Math.Max(0, Math.Min(progressBarPlayer1.Maximum, player1.Health));
            progressBarPlayer2.Value = Math.Max(0, Math.Min(progressBarPlayer2.Maximum, player2.Health));
        }


        private void txtBattleLog_TextChanged(object sender, EventArgs e)
        {
            // Empty handler - required by designer but no implementation needed
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Empty handler - required by designer but no implementation needed
        }

        private void UpdateBattleLog()
        {
            txtBattleLog.Clear();
            foreach (string log in battleManager.BattleLog)
            {
                txtBattleLog.AppendText(log + Environment.NewLine);
            }
            txtBattleLog.ScrollToCaret();
        }

        private void UpdateUI()
        {
            // Simple health display without advanced features
            if (battleManager.Player1 != null)
            {
                lblPlayer1Health.Text = $"Health: {battleManager.Player1.Health}";
                lblPlayer1Health.ForeColor = GetHealthColor(battleManager.Player1);
            }

            if (battleManager.Player2 != null)
            {
                lblPlayer2Health.Text = $"Health: {battleManager.Player2.Health}";
                lblPlayer2Health.ForeColor = GetHealthColor(battleManager.Player2);
            }
        }

        private Color GetHealthColor(CodingWarrior warrior)
        {
            if (warrior == null) return Color.Black;

            float healthPercent = (float)warrior.Health / warrior.MaxHealth;

            if (healthPercent > 0.7f)
                return Color.Green;
            else if (healthPercent > 0.3f)
                return Color.Orange;
            else
                return Color.Red;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // Prevent clicking during battle or animation
            if (battleTimer.Enabled || animatingPlayer != 0)
            {
                return; // Do nothing if battle is ongoing or animation is running
            }

            // Check if character types are selected
            if (cmbPlayer1Type.SelectedItem == null || cmbPlayer2Type.SelectedItem == null)
            {
                MessageBox.Show("Please select types for both players before viewing the fighting scene.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Get selected character types
            string player1Type = cmbPlayer1Type.SelectedItem.ToString();
            string player2Type = cmbPlayer2Type.SelectedItem.ToString();

            // Combine character images and display in pictureBox1
            pictureBox1.Image = CombineCharacterImages(player1Type, player2Type);
        }

        private Image CombineCharacterImages(string char1, string char2)
        {
            // Load character images from resources based on character names
            Image img1 = GetCharacterImage(char1);
            Image img2 = GetCharacterImage(char2);

            Debug.WriteLine($"CombineCharacterImages: Got image for {char1}: {img1 != null}, Size: {img1?.Size}");
            Debug.WriteLine($"CombineCharacterImages: Got image for {char2}: {img2 != null}, Size: {img2?.Size}");

            // Define a fixed padding from the bottom of the picture box
            int bottomPadding = 20; // Adjust as needed
            int groundLineY = pictureBox1.Height - bottomPadding; // Y coordinate for the bottom of the sprites

            Bitmap combined = new Bitmap(pictureBox1.Width, pictureBox1.Height); // Create bitmap with full picture box size
            using (Graphics g = Graphics.FromImage(combined))
            {
                g.Clear(Color.Transparent);

                int spacing = 50; // Increased spacing between characters

                // Calculate the available vertical space from the top of the picture box to the ground line
                // We will scale based on the full picture box height to ensure the full sprite is visible
                int availableHeightForScaling = pictureBox1.Height - bottomPadding; // Use area above padding

                // Calculate a scale factor that fits the sprite height within the available height while maintaining aspect ratio

                int scaledWidth1 = 0;
                int scaledHeight1 = 0;
                int scaledWidth2 = 0;
                int scaledHeight2 = 0;

                // Calculate Player 1's scaled dimensions
                if (img1 != null && availableHeightForScaling > 0)
                {
                    Debug.WriteLine($"CombineCharacterImages: Player 1 Original Dimensions: {{Width={img1.Width}, Height={img1.Height}}}");
                    // Calculate scale based on target height
                    float heightScale1 = (float)availableHeightForScaling / img1.Height;
                    // Calculate scale based on picture box width
                    float widthScale1 = (float)pictureBox1.Width / img1.Width;
                    // Use the minimum scale to maintain aspect ratio
                    float scale1 = Math.Min(heightScale1, widthScale1);
                    scaledWidth1 = (int)(img1.Width * scale1);
                    scaledHeight1 = (int)(img1.Height * scale1);
                    Debug.WriteLine($"CombineCharacterImages: Player 1 Scaled Dimensions: {{Width={scaledWidth1}, Height={scaledHeight1}}}");
                }

                // Calculate Player 2's scaled dimensions
                if (img2 != null && availableHeightForScaling > 0)
                {
                    Debug.WriteLine($"CombineCharacterImages: Player 2 Original Dimensions: {{Width={img2.Width}, Height={img2.Height}}}");
                     // Calculate scale based on target height
                     float heightScale2 = (float)availableHeightForScaling / img2.Height;
                      // Calculate scale based on picture box width
                      float widthScale2 = (float)pictureBox1.Width / img2.Width;
                      // Use the minimum scale to maintain aspect ratio
                      float scale2 = Math.Min(heightScale2, widthScale2);
                      scaledWidth2 = (int)(img2.Width * scale2);
                      scaledHeight2 = (int)(img2.Height * scale2);
                    Debug.WriteLine($"CombineCharacterImages: Player 2 Scaled Dimensions: {{Width={scaledWidth2}, Height={scaledHeight2}}}");
                }
                // --- End Calculate scaled dimensions ---

                // Total width occupied (consider cases where one image might be null or scaled to zero)
                int totalCharactersWidth = (scaledWidth1 > 0 ? scaledWidth1 : 0) + spacing + (scaledWidth2 > 0 ? scaledWidth2 : 0);
                int startX = (pictureBox1.Width - totalCharactersWidth) / 2;

                // Draw Player 1 image (scaled and positioned)
                if (img1 != null && scaledWidth1 > 0 && scaledHeight1 > 0)
                {
                    // Adjust position for Player 1
                    int x1 = startX;
                    // Position the bottom of the sprite relative to the bottom of the picture box (aligning to the ground)
                    int y1 = groundLineY - scaledHeight1; // Position top of sprite based on fixed ground line and scaled height

                    // Draw the scaled image
                    g.DrawImage(img1, new Rectangle(x1, y1, scaledWidth1, scaledHeight1));
                }

                // Draw Player 2 image (scaled and positioned)
                if (img2 != null && scaledWidth2 > 0 && scaledHeight2 > 0)
                {
                    // Flip Player 2's image horizontally to face Player 1
                    img2.RotateFlip(RotateFlipType.RotateNoneFlipX);

                    // Adjust position for Player 2
                    // Start X for player 2 depends on scaled width of player 1 and spacing
                    int x2 = startX + (scaledWidth1 > 0 ? scaledWidth1 : 0) + spacing;
                    // Position the bottom of the sprite relative to the bottom of the picture box (aligning to the ground)
                    int y2 = groundLineY - scaledHeight2; // Position top of sprite based on fixed ground line and scaled height

                    // Draw the scaled image
                    g.DrawImage(img2, new Rectangle(x2, y2, scaledWidth2, scaledHeight2));

                    // Flip the image back after drawing if you plan to reuse the Image object elsewhere
                    // This is important if img2 is a shared resource.
                    img2.RotateFlip(RotateFlipType.RotateNoneFlipX);
                }
            }

            return combined;
        }

        private Image GetCharacterImage(string characterType)
        {
            Debug.WriteLine($"GetCharacterImage: Requesting image for type: {characterType}");
            switch (characterType)
            {
                case "PauCoder":
                    Debug.WriteLine($"GetCharacterImage: Returning PauCoder_Idle: {Properties.Resources.PauCoder_Idle != null}");
                    // Return the first frame of the Idle animation as the default static image
                    if (Properties.Resources.PauCoder_Idle != null)
                    {
                        Debug.WriteLine($"GetCharacterImage: PauCoder_Idle resource found. Cloning first frame.");
                        Bitmap idleSpriteSheet = Properties.Resources.PauCoder_Idle;
                        Rectangle firstFrameRect = new Rectangle(0, 0, 128, 128); // First frame (128x128)
                        try
                        {
                            Image firstFrame = idleSpriteSheet.Clone(firstFrameRect, idleSpriteSheet.PixelFormat);
                            Debug.WriteLine($"GetCharacterImage: Cloned PauCoder_Idle first frame: {firstFrame != null}, Size: {firstFrame?.Size}");
                            return firstFrame;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"GetCharacterImage: Error cloning PauCoder_Idle first frame: {ex.Message}");
                            return null;
                        }
                    }
                    return null; // Or a default placeholder
                case "RogerRipper":
                    // Return the first frame of the Idle animation as the default static image for RogerRipper
                    // Assuming RogerRipper_Idle is 896x128 with 7 frames (128x128 per frame)
                    if (Properties.Resources.RogerRipper_Idle != null)
                    {
                        Debug.WriteLine($"GetCharacterImage: RogerRipper_Idle resource found.");
                        Bitmap idleSpriteSheet = Properties.Resources.RogerRipper_Idle;
                        Rectangle firstFrameRect = new Rectangle(0, 0, 128, 128); // First frame (128x128)
                        try
                        {
                            Image firstFrame = idleSpriteSheet.Clone(firstFrameRect, idleSpriteSheet.PixelFormat);
                            Debug.WriteLine($"GetCharacterImage: Cloned RogerRipper_Idle first frame: {firstFrame != null}, Size: {firstFrame?.Size}");
                            return firstFrame;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"GetCharacterImage: Error cloning RogerRipper_Idle first frame: {ex.Message}");
                            return null;
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"GetCharacterImage: RogerRipper_Idle resource NOT found.");
                        return null; // Or a default placeholder image
                    }
                case "StarLord":
                    Debug.WriteLine($"GetCharacterImage: Returning StarLord_Idle: {Properties.Resources.StarLord_Idle != null}");
                    // Return the first frame of the Idle animation as the default static image
                    if (Properties.Resources.StarLord_Idle != null)
                    {
                        Debug.WriteLine($"GetCharacterImage: StarLord_Idle resource found. Cloning first frame.");
                        Bitmap idleSpriteSheet = Properties.Resources.StarLord_Idle;
                        Rectangle firstFrameRect = new Rectangle(0, 0, 128, 128); // First frame (128x128)
                        try
                        {
                            Image firstFrame = idleSpriteSheet.Clone(firstFrameRect, idleSpriteSheet.PixelFormat);
                            Debug.WriteLine($"GetCharacterImage: Cloned StarLord_Idle first frame: {firstFrame != null}, Size: {firstFrame?.Size}");
                            return firstFrame;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"GetCharacterImage: Error cloning StarLord_Idle first frame: {ex.Message}");
                            return null;
                        }
                    }
                    return null; // Or a default placeholder
                case "RyenVizier":
                    Debug.WriteLine($"GetCharacterImage: Returning RyenVizier_Idle: {Properties.Resources.RyenVizier_Idle != null}");
                    // Return the first frame of the Idle animation as the default static image
                    if (Properties.Resources.RyenVizier_Idle != null)
                    {
                        Debug.WriteLine($"GetCharacterImage: RyenVizier_Idle resource found. Cloning first frame.");
                        Bitmap idleSpriteSheet = Properties.Resources.RyenVizier_Idle;
                        Rectangle firstFrameRect = new Rectangle(0, 0, 128, 128); // First frame (128x128)
                        try
                        {
                            Image firstFrame = idleSpriteSheet.Clone(firstFrameRect, idleSpriteSheet.PixelFormat);
                            Debug.WriteLine($"GetCharacterImage: Cloned RyenVizier_Idle first frame: {firstFrame != null}, Size: {firstFrame?.Size}");
                            return firstFrame;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"GetCharacterImage: Error cloning RyenVizier_Idle first frame: {ex.Message}");
                            return null;
                        }
                    }
                    return null; // Or a default placeholder
                default:
                    Debug.WriteLine($"GetCharacterImage: Unknown character type {characterType}");
                    return null;
            }
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            // Advance frame for the currently animating player
            if (animatingPlayer != 0)
            {
                int totalFrames = 0;
                int frameWidth = 0;
                int frameHeight = 0;

                if (animatingPlayer == 1)
                {
                    // Get details for Player 1's current action animation
                    if (player1CurrentAnimation != "Idle")
                    {
                        Image spriteSheet; // Declare variable
                        (spriteSheet, frameWidth, frameHeight) = GetAnimationDetails(player1CurrentAnimation);
                        totalFrames = GetAnimationTotalFrames(player1CurrentAnimation);
                        player1AnimationFrameIndex++;

                        // Check if Player 1's action animation has finished
                        if (player1AnimationFrameIndex >= totalFrames)
                        {
                            // If it's a death animation, stay on the last frame. Otherwise, transition to Idle.
                            if (player1CurrentAnimation.EndsWith("_Dead"))
                            {
                                player1AnimationFrameIndex = totalFrames - 1; // Stay on the last frame
                                animatingPlayer = 0; // Stop animating this player but keep dead state
                            }
                            else
                            {
                                player1CurrentAnimation = "Idle"; // Transition to Idle after non-death animation
                                player1AnimationFrameIndex = 0; // Reset frame index
                                animatingPlayer = 0; // No action animation is active
                            }
                        }
                    }
                     else
                    {
                        // Player 1 is in Idle but animatingPlayer is 1, shouldn't happen with new logic
                        animatingPlayer = 0;
                        player1AnimationFrameIndex = 0;
                    }
                }
                else if (animatingPlayer == 2)
                {
                    // Get details for Player 2's current action animation
                    if (player2CurrentAnimation != "Idle")
                    {
                        Image spriteSheet; // Declare variable
                        (spriteSheet, frameWidth, frameHeight) = GetAnimationDetails(player2CurrentAnimation);
                        totalFrames = GetAnimationTotalFrames(player2CurrentAnimation);
                        player2AnimationFrameIndex++;

                        // Check if Player 2's action animation has finished
                        if (player2AnimationFrameIndex >= totalFrames)
                        {
                             // If it's a death animation, stay on the last frame. Otherwise, transition to Idle.
                            if (player2CurrentAnimation.EndsWith("_Dead"))
                            {
                                player2AnimationFrameIndex = totalFrames - 1; // Stay on the last frame
                                animatingPlayer = 0; // Stop animating this player but keep dead state
                            }
                            else
                            {
                                player2CurrentAnimation = "Idle"; // Transition to Idle after non-death animation
                                player2AnimationFrameIndex = 0; // Reset frame index
                                animatingPlayer = 0; // No action animation is active
                            }
                        }
                    }
                    else
                    {
                        // Player 2 is in Idle but animatingPlayer is 2, shouldn't happen with new logic
                        animatingPlayer = 0;
                        player2AnimationFrameIndex = 0;
                    }
                }
            }
            else
            {
                // No action animation is currently playing. Check the queue for the next animation.
                if (animationQueue.Count > 0)
                {
                    var nextAnimation = animationQueue.Dequeue();
                    int playerToAnimate = nextAnimation.playerNumber;
                    string animationState = nextAnimation.animationState;

                    Debug.WriteLine($"AnimationTimer_Tick: Dequeued animation for Player {playerToAnimate} state: {animationState}");

                    // Start the animation using the dequeued request
                    // We need to get the sprite sheet and frame details here
                    Image spriteSheet; // Declare variable
                    int frameWidth = 0; // Declare and initialize variable
                    int frameHeight = 0; // Declare and initialize variable
                    (spriteSheet, frameWidth, frameHeight) = GetAnimationDetails(animationState);
                    int totalFrames = GetAnimationTotalFrames(animationState);

                    if (spriteSheet != null && totalFrames > 0)
                    {
                        // Call StartCharacterAnimation to set the state and frame index
                        StartCharacterAnimation(playerToAnimate, animationState, spriteSheet, totalFrames, frameWidth, frameHeight);
                         animatingPlayer = playerToAnimate; // Set animatingPlayer to the player whose animation is starting
                    }
                     else
                    {
                         Debug.WriteLine($"AnimationTimer_Tick: Failed to start animation {animationState}. Sprite sheet or total frames invalid.");
                    }
                }
            }

            // Always redraw on animation tick
            DrawCharactersBasedOnState();
        }

        private void DrawAnimationFrame(int frameIndex)
        {
            // This method is now more general and takes the sprite sheet and frame details as arguments
            // It draws a specific frame of a given sprite sheet onto a temporary bitmap
            // This bitmap will then be used by DrawCharacters to compose the final image
            throw new NotImplementedException(); // This method will be refactored or removed
        }

        // Method to start animation for a character action
        private void StartCharacterAnimation(int playerNumber, string animationState, Image spriteSheet, int totalFrames, int frameW, int frameH)
        {
            Debug.WriteLine($"StartCharacterAnimation called for Player {playerNumber} with state: {animationState}");

            // If this player is already animating this action, do nothing
            // With the queue, this check might be less critical but can stay as a safeguard.
            if ((playerNumber == 1 && player1CurrentAnimation == animationState && animatingPlayer == 1) || (playerNumber == 2 && player2CurrentAnimation == animationState && animatingPlayer == 2))
            {
                Debug.WriteLine($"StartCharacterAnimation: Player {playerNumber} already in state {animationState}.");
                return;
            }

            // We might need to re-evaluate the use of animatingPlayer if both can animate
            // For now, let's keep the state setting and remove the other player reset.

            Debug.WriteLine($"StartCharacterAnimation: Player {playerNumber} state set to {animationState}, frame index reset to 0.");

            // Ensure animation timer is running (it should be for idle, but good practice to check)
            if (!animationTimer.Enabled)
            {
                animationTimer.Start();
            }

            // Set the current player's animation state and reset frame index
            if (playerNumber == 1)
            {
                player1CurrentAnimation = animationState;
                player1AnimationFrameIndex = 0; // Start from the first frame of the new animation
                // animatingPlayer is now set in AnimationTimer_Tick when dequeuing
            }
            else if (playerNumber == 2)
            {
                player2CurrentAnimation = animationState;
                player2AnimationFrameIndex = 0; // Start from the first frame of the new animation
                 // animatingPlayer is now set in AnimationTimer_Tick when dequeuing
            }
        }

        // Helper method to get animation details based on state string
        private (Image spriteSheet, int frameWidth, int frameHeight) GetAnimationDetails(string animationState)
        {
            // Need to map animation state strings to sprite sheets and frame details
            // Add cases for all your action animations here
            switch (animationState)
            {
                case "RogerRipper_Attack_1":
                    return (Properties.Resources.RogerRipper_Attack_1, 128, 128); // Example details
                case "RogerRipper_Charge":
                    return (Properties.Resources.RogerRipper_Charge, 64, 64);
                case "RogerRipper_Hurt":
                    return (Properties.Resources.RogerRipper_Hurt, 128, 128);
                case "RogerRipper_Idle":
                    return (Properties.Resources.RogerRipper_Idle, 128, 128);
                // Add cases for other RogerRipper and other character animations
                // PauCoder Animations
                case "PauCoder_Attack_1": return (Properties.Resources.PauCoder_Attack_1, 128, 128);
                case "PauCoder_Attack_2": return (Properties.Resources.PauCoder_Attack_2, 128, 128);
                case "PauCoder_Charge": return (Properties.Resources.PauCoder_Charge, 64, 64); // Note: 64x64 frame size
                case "PauCoder_Dead": return (Properties.Resources.PauCoder_Dead, 128, 128);
                case "PauCoder_Hurt": return (Properties.Resources.PauCoder_Hurt, 128, 128);
                case "PauCoder_Idle": return (Properties.Resources.PauCoder_Idle, 128, 128);
                case "PauCoder_Jump": return (Properties.Resources.PauCoder_Jump, 128, 128);
                case "PauCoder_Light_ball": return (Properties.Resources.PauCoder_Light_ball, 128, 128);
                case "PauCoder_Light_charge": return (Properties.Resources.PauCoder_Light_charge, 128, 128);
                case "PauCoder_Run": return (Properties.Resources.PauCoder_Run, 128, 128);
                case "PauCoder_Walk": return (Properties.Resources.PauCoder_Walk, 128, 128);

                // RyenVizier Animations
                case "RyenVizier_Attack_1": return (Properties.Resources.RyenVizier_Attack_1, 128, 128);
                case "RyenVizier_Attack_2": return (Properties.Resources.RyenVizier_Attack_2, 128, 128);
                case "RyenVizier_Charge_1": return (Properties.Resources.RyenVizier_Charge_1, 128, 128);
                case "RyenVizier_Charge_2": return (Properties.Resources.RyenVizier_Charge_2, 128, 128);
                case "RyenVizier_Dead": return (Properties.Resources.RyenVizier_Dead, 128, 128);
                case "RyenVizier_Hurt": return (Properties.Resources.RyenVizier_Hurt, 128, 128);
                case "RyenVizier_Idle": return (Properties.Resources.RyenVizier_Idle, 128, 128);
                case "RyenVizier_Jump": return (Properties.Resources.RyenVizier_Jump, 128, 128);
                case "RyenVizier_Magic_arrow": return (Properties.Resources.RyenVizier_Magic_arrow, 128, 128);
                case "RyenVizier_Magic_sphere": return (Properties.Resources.RyenVizier_Magic_sphere, 128, 128);
                case "RyenVizier_Run": return (Properties.Resources.RyenVizier_Run, 128, 128);
                case "RyenVizier_Walk": return (Properties.Resources.RyenVizier_Walk, 128, 128);

                // StarLord Animations
                case "StarLord_Attack_1": return (Properties.Resources.StarLord_Attack_1, 128, 128);
                case "StarLord_Dead": return (Properties.Resources.StarLord_Dead, 128, 128);
                case "StarLord_Hurt": return (Properties.Resources.StarLord_Hurt, 128, 128);
                case "StarLord_Idle_2": return (Properties.Resources.StarLord_Idle_2, 128, 128);
                case "StarLord_Idle": return (Properties.Resources.StarLord_Idle, 128, 128);
                case "StarLord_Jump": return (Properties.Resources.StarLord_Jump, 128, 128);
                case "StarLord_Recharge": return (Properties.Resources.StarLord_Recharge, 128, 128);
                case "StarLord_Run": return (Properties.Resources.StarLord_Run, 128, 128);
                case "StarLord_Shot": return (Properties.Resources.StarLord_Shot, 128, 128);
                case "StarLord_Walk": return (Properties.Resources.StarLord_Walk, 128, 128);

                default:
                    return (null, 0, 0); // Return null for unknown states
            }
        }

        // Helper method to get total frames for an animation state
        private int GetAnimationTotalFrames(string animationState)
        {
            switch (animationState)
            {
                case "RogerRipper_Attack_1": return 4;
                case "RogerRipper_Charge": return 12;
                case "RogerRipper_Hurt": return 3;
                case "RogerRipper_Idle": return 7;
                // Add cases for other animations
                // PauCoder Animations (total width / frame width)
                case "PauCoder_Attack_1": return 1280 / 128; // 10 frames
                case "PauCoder_Attack_2": return 512 / 128;  // 4 frames
                case "PauCoder_Charge": return 576 / 64;   // 9 frames (Note: 64 frame width)
                case "PauCoder_Dead": return 640 / 128;    // 5 frames
                case "PauCoder_Hurt": return 384 / 128;    // 3 frames
                case "PauCoder_Idle": return 896 / 128;    // 7 frames
                case "PauCoder_Jump": return 1024 / 128;   // 8 frames
                case "PauCoder_Light_ball": return 896 / 128; // 7 frames
                case "PauCoder_Light_charge": return 1664 / 128; // 13 frames
                case "PauCoder_Run": return 1024 / 128;   // 8 frames
                case "PauCoder_Walk": return 896 / 128;    // 7 frames

                // RyenVizier Animations (total width / frame width 128)
                case "RyenVizier_Attack_1": return 896 / 128; // 7 frames
                case "RyenVizier_Attack_2": return 1152 / 128; // 9 frames
                case "RyenVizier_Charge_1": return 576 / 128; // 4 frames (approx, adjust if needed)
                case "RyenVizier_Charge_2": return 384 / 128; // 3 frames
                case "RyenVizier_Dead": return 512 / 128;    // 4 frames
                case "RyenVizier_Hurt": return 512 / 128;    // 4 frames
                case "RyenVizier_Idle": return 1024 / 128;   // 8 frames
                case "RyenVizier_Jump": return 1024 / 128;   // 8 frames
                case "RyenVizier_Magic_arrow": return 768 / 128; // 6 frames
                case "RyenVizier_Magic_sphere": return 2048 / 128; // 16 frames
                case "RyenVizier_Run": return 1024 / 128;   // 8 frames
                case "RyenVizier_Walk": return 896 / 128;    // 7 frames

                // StarLord Animations (total width / frame width 128)
                case "StarLord_Attack_1": return 384 / 128;  // 3 frames
                case "StarLord_Dead": return 640 / 128;    // 5 frames
                case "StarLord_Hurt": return 640 / 128;    // 5 frames
                case "StarLord_Idle_2": return 1408 / 128;   // 11 frames
                case "StarLord_Idle": return 768 / 128;    // 6 frames
                case "StarLord_Jump": return 1280 / 128;   // 10 frames
                case "StarLord_Recharge": return 2176 / 128; // 17 frames
                case "StarLord_Run": return 1280 / 128;   // 10 frames
                case "StarLord_Shot": return 512 / 128;    // 4 frames
                case "StarLord_Walk": return 1280 / 128;   // 10 frames

                default:
                    return 1; // Default to 1 frame for static/unknown
            }
        }

        // Helper method to get a list of all known animation states for a character type
        private List<string> GetKnownAnimationStates(string characterType)
        {
            List<string> states = new List<string>();
            // Add all known animation states for each character type here
            switch (characterType)
            {
                case "PauCoder":
                    states.AddRange(new[] { "PauCoder_Attack_1", "PauCoder_Attack_2", "PauCoder_Charge", "PauCoder_Dead", "PauCoder_Hurt", "PauCoder_Idle", "PauCoder_Jump", "PauCoder_Light_ball", "PauCoder_Light_charge", "PauCoder_Run", "PauCoder_Walk" });
                    break;
                case "RogerRipper":
                    states.AddRange(new[] { "RogerRipper_Attack_1", "RogerRipper_Charge", "RogerRipper_Hurt", "RogerRipper_Idle" }); // Add other RogerRipper states
                    // Adding all states from GetAnimationDetails for completeness
                    states.AddRange(new[] { "RogerRipper_Idle" }); // Already included, but for clarity
                    break;
                case "StarLord":
                    states.AddRange(new[] { "StarLord_Attack_1", "StarLord_Dead", "StarLord_Hurt", "StarLord_Idle_2", "StarLord_Idle", "StarLord_Jump", "StarLord_Recharge", "StarLord_Run", "StarLord_Shot", "StarLord_Walk" });
                    break;
                case "RyenVizier":
                    states.AddRange(new[] { "RyenVizier_Attack_1", "RyenVizier_Attack_2", "RyenVizier_Charge_1", "RyenVizier_Charge_2", "RyenVizier_Dead", "RyenVizier_Hurt", "RyenVizier_Idle", "RyenVizier_Jump", "RyenVizier_Magic_arrow", "RyenVizier_Magic_sphere", "RyenVizier_Run", "RyenVizier_Walk" });
                    break;
            }
            // Ensure distinct states in case of overlaps
            return new List<string>(new HashSet<string>(states));
        }

        private void txtBattleLog_TextChanged_1(object sender, EventArgs e)
        {

        }
    }
}